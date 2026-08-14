import React, { useEffect, useState } from 'react';
import { Box, Chip, Typography, Stack, Paper, Alert } from '@mui/material';
import Grid from '@mui/material/Grid2';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import axiosInstance from '../../services/axiosInstance';
import toast from 'react-hot-toast';

function formatDate(value) {
  if (!value) return '-';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return '-';
  return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function getStatus(row) {
  if (row.isReturned) return 'Returned';
  if (row.dueDate && new Date(row.dueDate) < new Date()) return 'Overdue';
  return 'Issued';
}

function StatCard({ label, value, color }) {
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Typography variant="body2" color="text.secondary">{label}</Typography>
      <Typography variant="h6" fontWeight={700} color={color}>
        {value}
      </Typography>
    </Paper>
  );
}

export default function MyIssuedBooksPage() {
  const [issues, setIssues] = useState([]);
  const [loading, setLoading] = useState(false);

  const fetchIssues = async () => {
    setLoading(true);
    try {
      const response = await axiosInstance.get('/library/my-issues');
      setIssues(response.data.data || []);
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to fetch your issued books');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchIssues();
  }, []);

  const issued = issues.filter((r) => getStatus(r) === 'Issued').length;
  const overdue = issues.filter((r) => getStatus(r) === 'Overdue').length;
  const returned = issues.filter((r) => getStatus(r) === 'Returned').length;

  const columns = [
    { id: 'bookTitle', header: 'Book', accessor: 'bookTitle', minWidth: 200 },
    { id: 'issueDate', header: 'Issue Date', accessor: (row) => formatDate(row.issueDate), minWidth: 120 },
    { id: 'dueDate', header: 'Due Date', accessor: (row) => formatDate(row.dueDate), minWidth: 120 },
    { id: 'returnedDate', header: 'Returned On', accessor: (row) => formatDate(row.returnedDate), minWidth: 120 },
    {
      id: 'status',
      header: 'Status',
      minWidth: 110,
      render: (value, row) => {
        const status = getStatus(row);
        return (
          <Chip
            label={status}
            color={status === 'Returned' ? 'success' : status === 'Overdue' ? 'error' : 'primary'}
            size="small"
            variant="outlined"
          />
        );
      },
    },
    {
      id: 'fineAmount',
      header: 'Fine',
      minWidth: 90,
      align: 'center',
      render: (value, row) => (row.fineAmount ? `$${Number(row.fineAmount).toFixed(2)}` : '-'),
    },
  ];

  return (
    <Box>
      <PageHeader
        title="My Issued Books"
        subtitle="Books currently issued to you from the library"
      />

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 4 }}>
          <StatCard label="Issued" value={issued} color="primary.main" />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <StatCard label="Overdue" value={overdue} color="error.main" />
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <StatCard label="Returned" value={returned} color="success.main" />
        </Grid>
      </Grid>

      {!loading && issues.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Stack spacing={1} alignItems="center">
            <MenuBookIcon sx={{ fontSize: 48, color: 'text.disabled' }} />
            <Alert severity="info" icon={false} sx={{ bgcolor: 'transparent', p: 0 }}>
              <Typography variant="body1" color="text.secondary">
                No books have been issued to you yet.
              </Typography>
            </Alert>
          </Stack>
        </Paper>
      ) : (
        <DataTable
          columns={columns}
          rows={issues}
          loading={loading}
          emptyMessage="No books have been issued to you yet"
          showActions={false}
        />
      )}
    </Box>
  );
}
