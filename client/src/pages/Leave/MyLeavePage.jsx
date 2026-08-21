import React, { useEffect, useState, useCallback } from 'react';
import { Box, Paper, Typography, Button, TextField, Dialog, DialogTitle, DialogContent, DialogActions, Table, TableHead, TableBody, TableRow, TableCell, Chip, CircularProgress, Alert, Grid, FormControl, InputLabel, Select, MenuItem } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import toast from 'react-hot-toast';
import leaveService from '../../services/leaveService';

const formatDateDDMMMYYYY = (dateStr) => {
  if (!dateStr) return '-';
  const d = new Date(dateStr);
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  return `${String(d.getDate()).padStart(2, '0')}-${months[d.getMonth()]}-${d.getFullYear()}`;
};

const countWeekdays = (from, to) => {
  if (!from || !to) return 0;
  const start = new Date(from);
  const end = new Date(to);
  let count = 0;
  const current = new Date(start);
  while (current <= end) {
    const day = current.getDay();
    if (day !== 0 && day !== 6) count++;
    current.setDate(current.getDate() + 1);
  }
  return count;
};

const statusColor = (status) => {
  switch (status) {
    case 'Pending': return 'warning';
    case 'Approved': return 'success';
    case 'Rejected': return 'error';
    case 'Cancelled': return 'default';
    default: return 'default';
  }
};

export default function MyLeavePage() {
  const [balance, setBalance] = useState([]);
  const [requests, setRequests] = useState([]);
  const [leaveTypes, setLeaveTypes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [cancelLoading, setCancelLoading] = useState(null);

  const [form, setForm] = useState({
    leaveTypeId: '',
    fromDate: '',
    toDate: '',
    reason: '',
    attachment: null,
  });

  const totalDays = countWeekdays(form.fromDate, form.toDate);

  const loadData = useCallback(async () => {
    try {
      const [balanceRes, requestsRes, typesRes] = await Promise.all([
        leaveService.getMyBalance(),
        leaveService.getMyRequests(),
        leaveService.getLeaveTypesForUser(),
      ]);
      const balData = balanceRes.data?.data ?? balanceRes.data ?? [];
      const reqData = requestsRes.data?.data ?? requestsRes.data ?? [];
      const typeData = typesRes.data?.data ?? typesRes.data ?? [];
      setBalance(Array.isArray(balData) ? balData : []);
      setRequests(Array.isArray(reqData) ? reqData : []);
      setLeaveTypes(Array.isArray(typeData) ? typeData : []);
    } catch {
      toast.error('Failed to load leave data');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadData(); }, [loadData]);

  const handleFormChange = (field, value) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleFileChange = (e) => {
    setForm((prev) => ({ ...prev, attachment: e.target.files[0] || null }));
  };

  const handleSubmit = async () => {
    if (!form.leaveTypeId) {
      toast.error('Please select a leave type');
      return;
    }
    if (!form.fromDate || !form.toDate) {
      toast.error('Please select from and to dates');
      return;
    }
    if (form.fromDate > form.toDate) {
      toast.error('From date must be before or equal to to date');
      return;
    }
    if (!form.reason.trim()) {
      toast.error('Please provide a reason');
      return;
    }

    setSubmitting(true);
    try {
      await leaveService.applyLeave({
        leaveTypeId: form.leaveTypeId,
        fromDate: form.fromDate,
        toDate: form.toDate,
        reason: form.reason.trim(),
      });
      toast.success('Leave applied successfully');
      setDialogOpen(false);
      setForm({ leaveTypeId: '', fromDate: '', toDate: '', reason: '', attachment: null });
      loadData();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to apply leave');
    } finally {
      setSubmitting(false);
    }
  };

  const handleCancel = async (id) => {
    setCancelLoading(id);
    try {
      await leaveService.cancelLeave(id);
      toast.success('Leave cancelled');
      loadData();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to cancel leave');
    } finally {
      setCancelLoading(null);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" fontWeight={700}>My Leave</Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => setDialogOpen(true)}>
          Apply Leave
        </Button>
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>Leave Balance</Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 700 }}>Leave Type</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700 }}>Allocated</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700 }}>Used</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700 }}>Pending</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700 }}>Remaining</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {balance.map((row, idx) => (
              <TableRow key={row.leaveTypeId || idx}>
                <TableCell>{row.leaveTypeName || row.name}</TableCell>
                <TableCell align="right">{row.allocatedDays ?? row.allocated ?? '-'}</TableCell>
                <TableCell align="right">{row.usedDays ?? row.used ?? '-'}</TableCell>
                <TableCell align="right">{row.pendingDays ?? row.pending ?? '-'}</TableCell>
                <TableCell align="right">{row.remainingDays ?? row.remaining ?? '-'}</TableCell>
              </TableRow>
            ))}
            {balance.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  <Typography variant="body2" color="text.secondary">No leave balance data</Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>My Leave Requests</Typography>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 700 }}>Leave Type</TableCell>
              <TableCell sx={{ fontWeight: 700 }}>From</TableCell>
              <TableCell sx={{ fontWeight: 700 }}>To</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700 }}>Days</TableCell>
              <TableCell sx={{ fontWeight: 700 }}>Reason</TableCell>
              <TableCell sx={{ fontWeight: 700 }}>Status</TableCell>
              <TableCell align="center" sx={{ fontWeight: 700 }}>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {requests.map((req) => (
              <TableRow key={req._id || req.id}>
                <TableCell>{req.leaveTypeName || req.leaveType?.name || '-'}</TableCell>
                <TableCell>{formatDateDDMMMYYYY(req.fromDate)}</TableCell>
                <TableCell>{formatDateDDMMMYYYY(req.toDate)}</TableCell>
                <TableCell align="right">{req.totalDays ?? '-'}</TableCell>
                <TableCell>{req.reason || '-'}</TableCell>
                <TableCell>
                  <Chip label={req.statusName || req.status} color={statusColor(req.statusName || req.status)} size="small" />
                </TableCell>
                <TableCell align="center">
                  {(req.statusName === 'Pending' || req.status === 1) && (
                    <Button
                      variant="outlined"
                      color="error"
                      size="small"
                      onClick={() => handleCancel(req._id || req.id)}
                      disabled={cancelLoading === (req._id || req.id)}
                    >
                      {cancelLoading === (req._id || req.id) ? <CircularProgress size={16} /> : 'Cancel'}
                    </Button>
                  )}
                </TableCell>
              </TableRow>
            ))}
            {requests.length === 0 && (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  <Typography variant="body2" color="text.secondary">No leave requests found</Typography>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </Paper>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Apply for Leave</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 0.5 }}>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>Leave Type *</InputLabel>
                <Select
                  label="Leave Type *"
                  value={form.leaveTypeId}
                  onChange={(e) => handleFormChange('leaveTypeId', e.target.value)}
                >
                  {leaveTypes.map((lt) => (
                    <MenuItem key={lt.id} value={lt.leaveTypeId || lt.id}>
                      {lt.leaveTypeName || lt.name} ({lt.leaveTypeCode || lt.code})
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                type="date"
                label="From Date *"
                value={form.fromDate}
                onChange={(e) => handleFormChange('fromDate', e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                type="date"
                label="To Date *"
                value={form.toDate}
                onChange={(e) => handleFormChange('toDate', e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            {form.fromDate && form.toDate && (
              <Grid item xs={12}>
                <Alert severity="info">
                  Total days (weekdays only): <strong>{totalDays}</strong>
                </Alert>
              </Grid>
            )}
            <Grid item xs={12}>
              <TextField
                fullWidth
                multiline
                rows={3}
                label="Reason *"
                value={form.reason}
                onChange={(e) => handleFormChange('reason', e.target.value)}
                required
              />
            </Grid>
            <Grid item xs={12}>
              <Button variant="outlined" component="label">
                {form.attachment ? form.attachment.name : 'Attach File (optional)'}
                <input type="file" hidden onChange={handleFileChange} />
              </Button>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setDialogOpen(false); setForm({ leaveTypeId: '', fromDate: '', toDate: '', reason: '', attachment: null }); }}>
            Cancel
          </Button>
          <Button variant="contained" onClick={handleSubmit} disabled={submitting || !form.reason.trim()}>
            {submitting ? <CircularProgress size={20} /> : 'Submit'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
