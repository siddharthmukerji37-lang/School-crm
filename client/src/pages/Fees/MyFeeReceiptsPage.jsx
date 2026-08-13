import React, { useEffect, useState } from 'react';
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Typography,
  Divider,
  Grid,
} from '@mui/material';
import ReceiptIcon from '@mui/icons-material/Receipt';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import axiosInstance from '../../services/axiosInstance';
import toast from 'react-hot-toast';

export default function MyFeeReceiptsPage() {
  const [receipts, setReceipts] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [detailOpen, setDetailOpen] = useState(false);
  const [selectedReceipt, setSelectedReceipt] = useState(null);

  const fetchReceipts = async () => {
    setLoading(true);
    try {
      const response = await axiosInstance.get('/fees/my-receipts', {
        params: { pageNumber: page + 1, pageSize: rowsPerPage },
      });
      setReceipts(response.data.data);
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to fetch receipts');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReceipts();
  }, [page, rowsPerPage]);

  const columns = [
    {
      id: 'receiptNumber',
      header: 'Receipt #',
      accessor: 'receiptNumber',
      minWidth: 130,
    },
    {
      id: 'feeStructureName',
      header: 'Fee',
      accessor: 'feeStructureName',
      minWidth: 150,
      render: (value, row) => value || row.feeType || 'N/A',
    },
    {
      id: 'amount',
      header: 'Amount',
      accessor: 'amount',
      minWidth: 90,
      align: 'right',
      render: (value) => `$${Number(value || 0).toFixed(2)}`,
    },
    {
      id: 'fineAmount',
      header: 'Fine',
      accessor: 'fineAmount',
      minWidth: 80,
      align: 'right',
      render: (value) => (
        <Typography
          variant="body2"
          color={value > 0 ? 'error.main' : 'text.secondary'}
        >
          ${Number(value || 0).toFixed(2)}
        </Typography>
      ),
    },
    {
      id: 'totalPaid',
      header: 'Total Paid',
      accessor: 'totalPaid',
      minWidth: 100,
      align: 'right',
      render: (value) => (
        <Typography variant="body2" fontWeight={600}>
          ${Number(value || 0).toFixed(2)}
        </Typography>
      ),
    },
    {
      id: 'paymentMethod',
      header: 'Method',
      accessor: 'paymentMethod',
      minWidth: 100,
      render: (value) => (
        <Chip label={value || 'N/A'} size="small" variant="outlined" />
      ),
    },
    {
      id: 'paymentDate',
      header: 'Date',
      accessor: 'paymentDate',
      minWidth: 110,
      render: (value) =>
        value ? new Date(value).toLocaleDateString() : 'N/A',
    },
  ];

  const handlePageChange = (_, newPage) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleViewReceipt = (row) => {
    setSelectedReceipt(row);
    setDetailOpen(true);
  };

  return (
    <Box>
      <PageHeader
        title="My Fee Receipts"
        subtitle={`Total ${receipts.totalCount || 0} receipts`}
      />

      <DataTable
        columns={columns}
        rows={receipts.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={receipts.totalCount || 0}
        searchPlaceholder="Search my receipts..."
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onView={handleViewReceipt}
        emptyMessage="No payment receipts found yet"
        showActions={true}
      />

      <Dialog
        open={detailOpen}
        onClose={() => {
          setDetailOpen(false);
          setSelectedReceipt(null);
        }}
        maxWidth="sm"
        fullWidth
        PaperProps={{ sx: { borderRadius: 2 } }}
      >
        <DialogTitle sx={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: 1 }}>
          <ReceiptIcon color="primary" />
          Payment Receipt
        </DialogTitle>
        <Divider />
        <DialogContent>
          {selectedReceipt && (
            <Grid container spacing={2}>
              <Grid size={{ xs: 12 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Receipt Number
                </Typography>
                <Typography variant="body1" fontWeight={600}>
                  {selectedReceipt.receiptNumber}
                </Typography>
              </Grid>
              <Grid size={{ xs: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Fee Structure
                </Typography>
                <Typography variant="body1">
                  {selectedReceipt.feeStructureName || 'N/A'}
                </Typography>
              </Grid>
              <Grid size={{ xs: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Fee Type
                </Typography>
                <Typography variant="body1">
                  {selectedReceipt.feeType || selectedReceipt.feeStructureName || 'N/A'}
                </Typography>
              </Grid>
              <Grid size={{ xs: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Amount
                </Typography>
                <Typography variant="body1" fontWeight={600}>
                  ${Number(selectedReceipt.amount || 0).toFixed(2)}
                </Typography>
              </Grid>
              <Grid size={{ xs: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Fine
                </Typography>
                <Typography
                  variant="body1"
                  color={selectedReceipt.fineAmount > 0 ? 'error.main' : 'inherit'}
                >
                  ${Number(selectedReceipt.fineAmount || 0).toFixed(2)}
                </Typography>
              </Grid>
              <Grid size={{ xs: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Total Paid
                </Typography>
                <Typography variant="body1" fontWeight={600} color="primary.main">
                  ${Number(selectedReceipt.totalPaid || 0).toFixed(2)}
                </Typography>
              </Grid>
              <Grid size={{ xs: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Payment Method
                </Typography>
                <Chip
                  label={selectedReceipt.paymentMethod || 'N/A'}
                  size="small"
                  variant="outlined"
                />
              </Grid>
              <Grid size={{ xs: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Payment Date
                </Typography>
                <Typography variant="body1">
                  {selectedReceipt.paymentDate
                    ? new Date(selectedReceipt.paymentDate).toLocaleDateString()
                    : 'N/A'}
                </Typography>
              </Grid>
              {selectedReceipt.transactionReference && (
                <Grid size={{ xs: 6 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Transaction Ref
                  </Typography>
                  <Typography variant="body1">
                    {selectedReceipt.transactionReference}
                  </Typography>
                </Grid>
              )}
              {selectedReceipt.remarks && (
                <Grid size={{ xs: 12 }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Remarks
                  </Typography>
                  <Typography variant="body1">
                    {selectedReceipt.remarks}
                  </Typography>
                </Grid>
              )}
            </Grid>
          )}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2.5 }}>
          <Button
            variant="outlined"
            onClick={() => {
              setDetailOpen(false);
              setSelectedReceipt(null);
            }}
          >
            Close
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
