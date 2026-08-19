import React, { useEffect, useState } from 'react';
import { Box, Paper, Typography, Button, Chip, TextField, MenuItem, CircularProgress, Alert, Dialog, DialogTitle, DialogContent, DialogActions } from '@mui/material';
import Grid from '@mui/material/Grid2';
import CheckIcon from '@mui/icons-material/Check';
import CloseIcon from '@mui/icons-material/Close';
import toast from 'react-hot-toast';
import salaryDeductionService from '../../services/salaryDeductionService';

const STATUS_OPTIONS = [
  { value: '', label: 'All' },
  { value: 'Pending', label: 'Pending' },
  { value: 'Approved', label: 'Approved' },
  { value: 'Rejected', label: 'Rejected' },
];

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

export default function SalaryDeductionPage() {
  const [deductions, setDeductions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState('Pending');
  const [monthFilter, setMonthFilter] = useState(new Date().getMonth() + 1);
  const [yearFilter, setYearFilter] = useState(new Date().getFullYear());
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogAction, setDialogAction] = useState('');
  const [selectedId, setSelectedId] = useState(null);

  const loadDeductions = async () => {
    setLoading(true);
    try {
      const res = await salaryDeductionService.getDeductions({
        month: monthFilter,
        year: yearFilter,
        status: statusFilter,
        page,
        pageSize: 20,
      });
      setDeductions(res.data.data?.items || []);
      setTotalCount(res.data.data?.totalCount || 0);
    } catch {
      setDeductions([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDeductions();
  }, [statusFilter, monthFilter, yearFilter, page]);

  const handleAction = async () => {
    try {
      if (dialogAction === 'approve') {
        await salaryDeductionService.approve(selectedId);
        toast.success('Deduction approved');
      } else {
        await salaryDeductionService.reject(selectedId);
        toast.success('Deduction rejected');
      }
      setDialogOpen(false);
      loadDeductions();
    } catch {
      toast.error('Action failed');
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Pending': return 'warning';
      case 'Approved': return 'success';
      case 'Rejected': return 'error';
      case 'Applied': return 'info';
      default: return 'default';
    }
  };

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} sx={{ mb: 3 }}>
        Salary Deductions
      </Typography>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid size={{ xs: 12, md: 3 }}>
            <TextField
              fullWidth size="small" select label="Status" value={statusFilter}
              onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
            >
              {STATUS_OPTIONS.map((s) => (
                <MenuItem key={s.value} value={s.value}>{s.label}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, md: 3 }}>
            <TextField
              fullWidth size="small" select label="Month" value={monthFilter}
              onChange={(e) => { setMonthFilter(parseInt(e.target.value)); setPage(1); }}
            >
              {MONTHS.map((m, i) => (
                <MenuItem key={i + 1} value={i + 1}>{m}</MenuItem>
              ))}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, md: 3 }}>
            <TextField
              fullWidth size="small" label="Year" type="number" value={yearFilter}
              onChange={(e) => { setYearFilter(parseInt(e.target.value)); setPage(1); }}
            />
          </Grid>
        </Grid>
      </Paper>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : deductions.length === 0 ? (
        <Alert severity="info">No salary deductions found for the selected filters.</Alert>
      ) : (
        <Paper sx={{ overflow: 'hidden' }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse' }}>
              <Box component="thead">
                <Box component="tr" sx={{ borderBottom: '1px solid', borderColor: 'divider', bgcolor: 'grey.50' }}>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Employee</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Date</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Late Count</Box>
                  <Box component="th" sx={{ textAlign: 'right', py: 1.5, px: 2, fontWeight: 600 }}>Amount</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Reason</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Status</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Action</Box>
                </Box>
              </Box>
              <Box component="tbody">
                {deductions.map((d) => (
                  <Box component="tr" key={d.id} sx={{ borderBottom: '1px solid', borderColor: 'divider', '&:hover': { bgcolor: 'grey.50' } }}>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{d.userName}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{new Date(d.attendanceDate).toLocaleDateString()}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center' }}>{d.lateCountMonth} / {d.allowedLateCount}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'right' }}>${d.deductionAmount.toFixed(2)}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{d.reason}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center' }}>
                      <Chip label={d.status} color={getStatusColor(d.status)} size="small" />
                    </Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center' }}>
                      {d.status === 'Pending' && (
                        <Box sx={{ display: 'flex', gap: 0.5, justifyContent: 'center' }}>
                          <Button size="small" color="success" startIcon={<CheckIcon />} onClick={() => { setSelectedId(d.id); setDialogAction('approve'); setDialogOpen(true); }}>Approve</Button>
                          <Button size="small" color="error" startIcon={<CloseIcon />} onClick={() => { setSelectedId(d.id); setDialogAction('reject'); setDialogOpen(true); }}>Reject</Button>
                        </Box>
                      )}
                    </Box>
                  </Box>
                ))}
              </Box>
            </Box>
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'center', gap: 1, py: 2 }}>
            <Button disabled={page <= 1} onClick={() => setPage(page - 1)}>Previous</Button>
            <Typography sx={{ alignSelf: 'center' }}>Page {page}</Typography>
            <Button disabled={deductions.length < 20} onClick={() => setPage(page + 1)}>Next</Button>
          </Box>
        </Paper>
      )}

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)}>
        <DialogTitle>{dialogAction === 'approve' ? 'Approve Deduction' : 'Reject Deduction'}</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to {dialogAction} this salary deduction?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleAction} color={dialogAction === 'approve' ? 'success' : 'error'} variant="contained">
            {dialogAction === 'approve' ? 'Approve' : 'Reject'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
