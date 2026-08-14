import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip, MenuItem, TextField, Stack } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchAttendance } from '../../store/slices/attendanceSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';

const STATUS_OPTIONS = [
  { value: '', label: 'All Status' },
  { value: 'Present', label: 'Present' },
  { value: 'Absent', label: 'Absent' },
  { value: 'Late', label: 'Late' },
  { value: 'Excused', label: 'Excused' },
];

const statusColor = (status) => {
  switch (status) {
    case 'Present': return 'success';
    case 'Absent': return 'error';
    case 'Late': return 'warning';
    case 'Excused': return 'info';
    default: return 'default';
  }
};

export default function AttendanceListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { records, loading } = useSelector((state) => state.attendance);

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [dateFilter, setDateFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  useEffect(() => {
    const params = { page: page + 1, pageSize: rowsPerPage };
    if (dateFilter) params.date = dateFilter;
    if (statusFilter) params.status = statusFilter;
    dispatch(fetchAttendance(params));
  }, [dispatch, page, rowsPerPage, dateFilter, statusFilter]);

  const items = records?.items || [];
  const totalCount = records?.totalCount || 0;

  const columns = [
    { id: 'date', header: 'Date', accessor: 'date', minWidth: 110 },
    { id: 'studentName', header: 'Student', accessor: 'studentName', minWidth: 160 },
    { id: 'className', header: 'Class', accessor: 'className', minWidth: 100 },
    { id: 'sectionName', header: 'Section', accessor: 'sectionName', minWidth: 90 },
    {
      id: 'status',
      header: 'Status',
      accessor: 'status',
      minWidth: 100,
      render: (value) => (
        <Chip label={value} color={statusColor(value)} size="small" variant="outlined" />
      ),
    },
    { id: 'remarks', header: 'Remarks', accessor: 'remarks', minWidth: 150 },
  ];

  return (
    <Box>
      <PageHeader
        title="Attendance"
        subtitle={`Total ${totalCount} records`}
        actions={
          <Stack direction="column" spacing={2} alignItems="stretch" sx={{ minWidth: 200 }}>
            <TextField
              size="small"
              label="Filter by Date"
              type="date"
              value={dateFilter}
              onChange={(e) => { setDateFilter(e.target.value); setPage(0); }}
              slotProps={{ inputLabel: { shrink: true } }}
              sx={{ minWidth: 160 }}
            />
            <TextField
              select
              size="small"
              label="Status"
              value={statusFilter}
              onChange={(e) => { setStatusFilter(e.target.value); setPage(0); }}
              sx={{ minWidth: 140 }}
            >
              {STATUS_OPTIONS.map((opt) => (
                <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
              ))}
            </TextField>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => navigate('/attendance/mark')}
            >
              Mark Attendance
            </Button>
          </Stack>
        }
      />
      <DataTable
        columns={columns}
        rows={items}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={totalCount}
        searchPlaceholder="Search attendance..."
        onPageChange={(_, p) => setPage(p)}
        onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
        emptyMessage="No attendance records found"
      />
    </Box>
  );
}
