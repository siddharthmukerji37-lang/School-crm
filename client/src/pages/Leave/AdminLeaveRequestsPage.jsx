import React, { useEffect, useState } from 'react';
import { Box, Paper, Typography, Button, Chip, IconButton, TextField, MenuItem, CircularProgress, Alert, Dialog, DialogTitle, DialogContent, DialogActions } from '@mui/material';
import Grid from '@mui/material/Grid2';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import VisibilityIcon from '@mui/icons-material/Visibility';
import toast from 'react-hot-toast';
import leaveService from '../../services/leaveService';

const STATUS_OPTIONS = [
  { value: '', label: 'All' },
  { value: 'Pending', label: 'Pending' },
  { value: 'Approved', label: 'Approved' },
  { value: 'Rejected', label: 'Rejected' },
];

const formatDate = (dateStr) => {
  if (!dateStr) return '-';
  const d = new Date(dateStr);
  const day = String(d.getDate()).padStart(2, '0');
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  return `${day}-${months[d.getMonth()]}-${d.getFullYear()}`;
};

export default function AdminLeaveRequestsPage() {
  const [requests, setRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState('Pending');
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 20;

  const [viewDialog, setViewDialog] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState(null);

  const [approveDialog, setApproveDialog] = useState(false);
  const [approveId, setApproveId] = useState(null);
  const [approveReason, setApproveReason] = useState('');

  const [rejectDialog, setRejectDialog] = useState(false);
  const [rejectId, setRejectId] = useState(null);
  const [rejectReason, setRejectReason] = useState('');

  const loadRequests = async () => {
    setLoading(true);
    try {
      const params = { page, pageSize };
      if (statusFilter) params.status = statusFilter;
      const res = await leaveService.getAllRequests(params);
      setRequests(res.data.data?.items || []);
      setTotalCount(res.data.data?.totalCount || 0);
    } catch {
      setRequests([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRequests();
  }, [statusFilter, page]);

  const handleApprove = async () => {
    try {
      await leaveService.approveLeave(approveId, { adminReason: approveReason });
      toast.success('Leave approved');
      setApproveDialog(false);
      setApproveId(null);
      setApproveReason('');
      loadRequests();
    } catch {
      toast.error('Failed to approve leave');
    }
  };

  const handleReject = async () => {
    if (!rejectReason.trim()) {
      toast.error('Admin reason is required');
      return;
    }
    try {
      await leaveService.rejectLeave(rejectId, { adminReason: rejectReason });
      toast.success('Leave rejected');
      setRejectDialog(false);
      setRejectId(null);
      setRejectReason('');
      loadRequests();
    } catch {
      toast.error('Failed to reject leave');
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Pending': return 'warning';
      case 'Approved': return 'success';
      case 'Rejected': return 'error';
      case 'Cancelled': return 'default';
      default: return 'default';
    }
  };

  const openViewDialog = (req) => {
    setSelectedRequest(req);
    setViewDialog(true);
  };

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} sx={{ mb: 3 }}>
        Leave Requests
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
        </Grid>
      </Paper>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
          <CircularProgress />
        </Box>
      ) : requests.length === 0 ? (
        <Alert severity="info">No leave requests found for the selected filters.</Alert>
      ) : (
        <Paper sx={{ overflow: 'hidden' }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse' }}>
              <Box component="thead">
                <Box component="tr" sx={{ borderBottom: '1px solid', borderColor: 'divider', bgcolor: 'grey.50' }}>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Name</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>User Type</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Leave Type</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>From Date</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>To Date</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Total Days</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Reason</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Status</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Applied Date</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Actions</Box>
                </Box>
              </Box>
              <Box component="tbody">
                {requests.map((req) => (
                  <Box component="tr" key={req.id} sx={{ borderBottom: '1px solid', borderColor: 'divider', '&:hover': { bgcolor: 'grey.50' } }}>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{req.userName}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{req.userType}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{req.leaveTypeName}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{formatDate(req.fromDate)}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{formatDate(req.toDate)}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center' }}>{req.totalDays}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{req.reason}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center' }}>
                      <Chip label={req.statusName} color={getStatusColor(req.statusName)} size="small" />
                    </Box>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{formatDate(req.appliedDate)}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center' }}>
                      <Box sx={{ display: 'flex', gap: 0.5, justifyContent: 'center' }}>
                        <IconButton size="small" color="info" onClick={() => openViewDialog(req)}>
                          <VisibilityIcon fontSize="small" />
                        </IconButton>
                        {req.statusName === 'Pending' && (
                          <>
                            <IconButton size="small" color="success" onClick={() => { setApproveId(req.id); setApproveDialog(true); }}>
                              <CheckCircleIcon fontSize="small" />
                            </IconButton>
                            <IconButton size="small" color="error" onClick={() => { setRejectId(req.id); setRejectDialog(true); }}>
                              <CancelIcon fontSize="small" />
                            </IconButton>
                          </>
                        )}
                      </Box>
                    </Box>
                  </Box>
                ))}
              </Box>
            </Box>
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'center', gap: 1, py: 2 }}>
            <Button disabled={page <= 1} onClick={() => setPage(page - 1)}>Previous</Button>
            <Typography sx={{ alignSelf: 'center' }}>Page {page}</Typography>
            <Button disabled={requests.length < pageSize} onClick={() => setPage(page + 1)}>Next</Button>
          </Box>
        </Paper>
      )}

      {/* View Dialog */}
      <Dialog open={viewDialog} onClose={() => setViewDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Leave Request Details</DialogTitle>
        <DialogContent dividers>
          {selectedRequest && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Box><Typography variant="caption" color="text.secondary">Name</Typography><Typography>{selectedRequest.userName}</Typography></Box>
              <Box><Typography variant="caption" color="text.secondary">User Type</Typography><Typography>{selectedRequest.userType}</Typography></Box>
              <Box><Typography variant="caption" color="text.secondary">Leave Type</Typography><Typography>{selectedRequest.leaveTypeName}</Typography></Box>
              <Box><Typography variant="caption" color="text.secondary">From Date</Typography><Typography>{formatDate(selectedRequest.fromDate)}</Typography></Box>
              <Box><Typography variant="caption" color="text.secondary">To Date</Typography><Typography>{formatDate(selectedRequest.toDate)}</Typography></Box>
              <Box><Typography variant="caption" color="text.secondary">Total Days</Typography><Typography>{selectedRequest.totalDays}</Typography></Box>
              <Box><Typography variant="caption" color="text.secondary">Reason</Typography><Typography>{selectedRequest.reason}</Typography></Box>
              <Box><Typography variant="caption" color="text.secondary">Status</Typography><Chip label={selectedRequest.statusName} color={getStatusColor(selectedRequest.statusName)} size="small" sx={{ mt: 0.5 }} /></Box>
              <Box><Typography variant="caption" color="text.secondary">Applied Date</Typography><Typography>{formatDate(selectedRequest.appliedDate)}</Typography></Box>
              {selectedRequest.adminReason && (
                <Box><Typography variant="caption" color="text.secondary">Admin Reason</Typography><Typography>{selectedRequest.adminReason}</Typography></Box>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewDialog(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      {/* Approve Dialog */}
      <Dialog open={approveDialog} onClose={() => setApproveDialog(false)}>
        <DialogTitle>Approve Leave</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>Are you sure you want to approve this leave request?</Typography>
          <TextField
            fullWidth size="small" label="Reason (optional)" multiline rows={2}
            value={approveReason} onChange={(e) => setApproveReason(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setApproveDialog(false); setApproveReason(''); }}>Cancel</Button>
          <Button onClick={handleApprove} color="success" variant="contained">Approve</Button>
        </DialogActions>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog open={rejectDialog} onClose={() => setRejectDialog(false)}>
        <DialogTitle>Reject Leave</DialogTitle>
        <DialogContent>
          <Typography sx={{ mb: 2 }}>Please provide a reason for rejecting this leave request.</Typography>
          <TextField
            fullWidth size="small" label="Admin Reason (required)" multiline rows={2} required
            value={rejectReason} onChange={(e) => setRejectReason(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setRejectDialog(false); setRejectReason(''); }}>Cancel</Button>
          <Button onClick={handleReject} color="error" variant="contained">Reject</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
