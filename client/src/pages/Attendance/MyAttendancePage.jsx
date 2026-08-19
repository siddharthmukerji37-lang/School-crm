import React, { useEffect, useState, useCallback } from 'react';
import { Box, Paper, Typography, Button, CircularProgress, Chip, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Alert } from '@mui/material';
import Grid from '@mui/material/Grid2';
import LoginIcon from '@mui/icons-material/Login';
import LogoutIcon from '@mui/icons-material/Logout';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import attendanceService from '../../services/attendanceService';
import toast from 'react-hot-toast';

export default function MyAttendancePage() {
  const [record, setRecord] = useState(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [currentTime, setCurrentTime] = useState(new Date());
  const [lateDialogOpen, setLateDialogOpen] = useState(false);
  const [lateReason, setLateReason] = useState('');
  const [lateMinutes, setLateMinutes] = useState(0);
  const [warningData, setWarningData] = useState(null);
  const [earlyDialogOpen, setEarlyDialogOpen] = useState(false);
  const [earlyReason, setEarlyReason] = useState('');
  const [earlyMinutes, setEarlyMinutes] = useState(0);
  const [earlyWarningData, setEarlyWarningData] = useState(null);

  const SCHOOL_END_HOUR = 18;
  const SCHOOL_END_MINUTE = 30;

  useEffect(() => {
    const timer = setInterval(() => setCurrentTime(new Date()), 1000);
    return () => clearInterval(timer);
  }, []);

  const loadRecord = useCallback(async () => {
    try {
      const res = await attendanceService.getMy();
      setRecord(res.data.data);
    } catch {
      setRecord(null);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadRecord(); }, [loadRecord]);

  const calculateLateMinutes = () => {
    const now = new Date();
    const cutoff = new Date();
    cutoff.setHours(9, 30, 0, 0);
    if (now > cutoff) {
      return Math.floor((now - cutoff) / 60000);
    }
    return 0;
  };

  const handleClockIn = async () => {
    const now = new Date();
    const cutoff = new Date();
    cutoff.setHours(9, 30, 0, 0);

    if (now > cutoff) {
      const mins = Math.floor((now - cutoff) / 60000);
      setLateMinutes(mins);
      setLateDialogOpen(true);
    } else {
      setActionLoading(true);
      try {
        const res = await attendanceService.clockIn();
        const data = res.data.data;
        setRecord(data);
        toast.success('Clocked in successfully');
      } catch (err) {
        toast.error(err.response?.data?.message || 'Failed to clock in');
      } finally {
        setActionLoading(false);
      }
    }
  };

  const handleLateSubmit = async () => {
    if (!lateReason.trim()) {
      toast.error('Please provide a reason for being late');
      return;
    }
    setActionLoading(true);
    setLateDialogOpen(false);
    try {
      const res = await attendanceService.clockInWithReason({ lateReason: lateReason.trim() });
      const data = res.data.data;
      setRecord(data);

      if (data.policyExceeded) {
        setWarningData(data);
        toast.error(`Warning: You have exceeded the allowed ${data.allowedLateCount} late arrivals this month. Late Count: ${data.lateCount}/${data.allowedLateCount}. Salary deduction may apply.`, { duration: 8000 });
      } else if (data.lateCount === data.allowedLateCount) {
        toast(`Notice: This is your ${data.lateCount}th late arrival. You have reached the maximum allowed late arrivals for this month.`, { icon: '⚠️', duration: 6000 });
      } else {
        toast.success(`Clocked in (${data.lateMinutes} min late). Late count: ${data.lateCount}/${data.allowedLateCount}`);
      }
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to clock in');
    } finally {
      setActionLoading(false);
      setLateReason('');
    }
  };

  const handleClockOut = async () => {
    const now = new Date();
    const end = new Date();
    end.setHours(SCHOOL_END_HOUR, SCHOOL_END_MINUTE, 0, 0);

    if (now < end) {
      const mins = Math.floor((end - now) / 60000);
      setEarlyMinutes(mins);
      setEarlyDialogOpen(true);
    } else {
      setActionLoading(true);
      try {
        const res = await attendanceService.clockOut();
        setRecord(res.data.data);
        toast.success('Clocked out successfully');
      } catch (err) {
        toast.error(err.response?.data?.message || 'Failed to clock out');
      } finally {
        setActionLoading(false);
      }
    }
  };

  const handleEarlySubmit = async () => {
    if (!earlyReason.trim()) {
      toast.error('Please provide a reason for early departure');
      return;
    }
    setActionLoading(true);
    setEarlyDialogOpen(false);
    try {
      const res = await attendanceService.clockOutWithReason({ earlyReason: earlyReason.trim() });
      const data = res.data.data;
      setRecord(data);
      if (data.earlyWarning) {
        setEarlyWarningData(data);
        toast(data.earlyWarning, { icon: '⚠️', duration: 6000 });
      } else {
        toast.success(`Clocked out. Left ${data.earlyMinutes} minutes early.`);
      }
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to clock out');
    } finally {
      setActionLoading(false);
      setEarlyReason('');
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const formatTime = (date) => date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  const formatDate = (date) => date.toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} sx={{ mb: 3 }}>
        My Attendance
      </Typography>

      <Paper sx={{ p: 4, mb: 3, textAlign: 'center' }}>
        <AccessTimeIcon sx={{ fontSize: 48, color: 'primary.main', mb: 1 }} />
        <Typography variant="h2" fontWeight={700} sx={{ mb: 0.5 }}>
          {formatTime(currentTime)}
        </Typography>
        <Typography variant="body1" color="text.secondary">
          {formatDate(currentTime)}
        </Typography>
      </Paper>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 3, textAlign: 'center' }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Clock In
            </Typography>
            {record?.isCheckedIn ? (
              <Box>
                <Chip label="Clocked In" color="success" sx={{ mb: 2 }} />
                <Typography variant="body2" color="text.secondary">
                  At: {record.checkInTime ? `${String(record.checkInTime.hours || 0).padStart(2, '0')}:${String(record.checkInTime.minutes || 0).padStart(2, '0')}` : '-'}
                </Typography>
                {record.lateMinutes > 0 && (
                  <Typography variant="body2" color="error.main" sx={{ mt: 1 }}>
                    {record.lateMinutes} minutes late
                  </Typography>
                )}
              </Box>
            ) : (
              <Button
                variant="contained"
                size="large"
                startIcon={actionLoading ? <CircularProgress size={20} /> : <LoginIcon />}
                onClick={handleClockIn}
                disabled={actionLoading}
                sx={{ mt: 1 }}
              >
                Clock In
              </Button>
            )}
          </Paper>
        </Grid>
        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 3, textAlign: 'center' }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Clock Out
            </Typography>
            {record?.isCheckedOut ? (
              <Chip label="Clocked Out" color="default" sx={{ mb: 2 }} />
            ) : (
              <Button
                variant="outlined"
                size="large"
                startIcon={actionLoading ? <CircularProgress size={20} /> : <LogoutIcon />}
                onClick={handleClockOut}
                disabled={actionLoading || !record?.isCheckedIn}
                sx={{ mt: 1 }}
              >
                Clock Out
              </Button>
            )}
          </Paper>
        </Grid>
      </Grid>

      {warningData && (
        <Alert severity="warning" sx={{ mt: 3 }} onClose={() => setWarningData(null)}>
          <Typography variant="subtitle2" fontWeight={600}>Salary Deduction Warning</Typography>
          You have exceeded the allowed {warningData.allowedLateCount} late arrivals this month.
          Late Count: {warningData.lateCount}/{warningData.allowedLateCount}.
          Salary deduction may apply according to school policy.
        </Alert>
      )}

      <Dialog open={lateDialogOpen} onClose={() => setLateDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WarningAmberIcon color="warning" />
          Late Arrival
        </DialogTitle>
        <DialogContent>
          <Box sx={{ mb: 2, mt: 1 }}>
            <Typography variant="body1">
              <strong>Scheduled Time:</strong> 9:30 AM
            </Typography>
            <Typography variant="body1">
              <strong>Current Time:</strong> {currentTime.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}
            </Typography>
            <Typography variant="body1">
              <strong>Late By:</strong> {lateMinutes} minutes
            </Typography>
          </Box>
          {warningData && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              You have exceeded the allowed number of late arrivals this month.
            </Alert>
          )}
          <TextField
            fullWidth
            multiline
            rows={3}
            label="Reason for being late *"
            value={lateReason}
            onChange={(e) => setLateReason(e.target.value)}
            placeholder="Please provide a reason for your late arrival..."
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setLateDialogOpen(false); setLateReason(''); }}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleLateSubmit}
            disabled={!lateReason.trim() || actionLoading}
          >
            {actionLoading ? <CircularProgress size={20} /> : 'Submit & Clock In'}
          </Button>
        </DialogActions>
      </Dialog>

      {earlyWarningData && (
        <Alert severity="warning" sx={{ mt: 3 }} onClose={() => setEarlyWarningData(null)}>
          <Typography variant="subtitle2" fontWeight={600}>Early Departure Notice</Typography>
          You left {earlyWarningData.earlyMinutes} minutes early.
          Your early departure has been recorded.
        </Alert>
      )}

      <Dialog open={earlyDialogOpen} onClose={() => setEarlyDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WarningAmberIcon color="warning" />
          Early Departure
        </DialogTitle>
        <DialogContent>
          <Box sx={{ mb: 2, mt: 1 }}>
            <Typography variant="body1">
              <strong>Scheduled End Time:</strong> 6:30 PM
            </Typography>
            <Typography variant="body1">
              <strong>Current Time:</strong> {currentTime.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' })}
            </Typography>
            <Typography variant="body1">
              <strong>Leaving Early By:</strong> {earlyMinutes} minutes
            </Typography>
          </Box>
          <TextField
            fullWidth
            multiline
            rows={3}
            label="Reason for early departure *"
            value={earlyReason}
            onChange={(e) => setEarlyReason(e.target.value)}
            placeholder="Please provide a reason for leaving early..."
            sx={{ mt: 1 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setEarlyDialogOpen(false); setEarlyReason(''); }}>Cancel</Button>
          <Button
            variant="contained"
            onClick={handleEarlySubmit}
            disabled={!earlyReason.trim() || actionLoading}
          >
            {actionLoading ? <CircularProgress size={20} /> : 'Submit & Clock Out'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
