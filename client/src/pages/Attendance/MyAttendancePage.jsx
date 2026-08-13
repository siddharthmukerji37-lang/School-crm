import React, { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Button, Typography, Paper, Chip, Stack, CircularProgress, Divider,
} from '@mui/material';
import LoginIcon from '@mui/icons-material/Login';
import LogoutIcon from '@mui/icons-material/Logout';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import toast from 'react-hot-toast';
import attendanceService from '../../services/attendanceService';

const formatTime = (time) => {
  if (!time) return '—';
  const str = String(time);
  const parts = str.split(':').map((p) => parseInt(p, 10));
  if (parts.length < 2 || isNaN(parts[0])) return str;
  const hours = parts[0];
  const minutes = parts[1];
  const suffix = hours >= 12 ? 'PM' : 'AM';
  const displayHours = hours % 12 === 0 ? 12 : hours % 12;
  return `${String(displayHours).padStart(2, '0')}:${String(minutes).padStart(2, '0')} ${suffix}`;
};

const computeDuration = (checkIn, checkOut) => {
  if (!checkIn || !checkOut) return '—';
  const toSeconds = (time) => {
    const parts = String(time).split(':').map((p) => parseInt(p, 10));
    return (parts[0] || 0) * 3600 + (parts[1] || 0) * 60 + (parts[2] || 0);
  };
  let seconds = toSeconds(checkOut) - toSeconds(checkIn);
  if (seconds < 0) seconds += 24 * 3600;
  const h = Math.floor(seconds / 3600);
  const m = Math.floor((seconds % 3600) / 60);
  return `${String(h).padStart(2, '0')}h ${String(m).padStart(2, '0')}m`;
};

export default function MyAttendancePage() {
  const navigate = useNavigate();
  const [record, setRecord] = useState(null);
  const [loading, setLoading] = useState(true);
  const [acting, setActing] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const res = await attendanceService.getMy();
      setRecord(res.data.data);
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to load your attendance');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const handleClockIn = async () => {
    setActing(true);
    try {
      const res = await attendanceService.clockIn();
      setRecord(res.data.data);
      toast.success('Clocked in successfully');
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to clock in');
    } finally {
      setActing(false);
    }
  };

  const handleClockOut = async () => {
    setActing(true);
    try {
      const res = await attendanceService.clockOut();
      setRecord(res.data.data);
      toast.success('Clocked out successfully');
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to clock out');
    } finally {
      setActing(false);
    }
  };

  const statusColor =
    record?.status === 'Present' ? 'success' : record?.status === 'Absent' ? 'error' : 'default';

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/dashboard')} variant="outlined">
          Back
        </Button>
        <Typography variant="h4" fontWeight={700}>My Attendance</Typography>
      </Box>

      <Paper sx={{ p: 4, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1 }}>
          <AccessTimeIcon color="primary" />
          <Typography variant="h6" fontWeight={600}>
            Today&apos;s Status
          </Typography>
        </Box>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          Your presence is counted when you sign in. Use the buttons below to clock in and clock out.
        </Typography>
        <Divider sx={{ mb: 3 }} />

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Stack spacing={3}>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems="center">
              <Chip
                label={record?.status || 'Not Marked'}
                color={statusColor}
                variant={record?.status === 'Present' ? 'filled' : 'outlined'}
              />
              <Typography variant="body2" color="text.secondary">
                {new Date().toLocaleDateString()}
              </Typography>
            </Stack>

            <Stack
              direction={{ xs: 'column', sm: 'row' }}
              spacing={2}
              sx={{ '& .MuiPaper-root': { flex: 1, p: 2, textAlign: 'center' } }}
            >
              <Paper variant="outlined">
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Clock In
                </Typography>
                <Typography variant="h5" fontWeight={700} color={record?.checkInTime ? 'success.main' : 'text.secondary'}>
                  {formatTime(record?.checkInTime)}
                </Typography>
              </Paper>
              <Paper variant="outlined">
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Clock Out
                </Typography>
                <Typography variant="h5" fontWeight={700} color={record?.checkOutTime ? 'error.main' : 'text.secondary'}>
                  {formatTime(record?.checkOutTime)}
                </Typography>
              </Paper>
              <Paper variant="outlined">
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Duration
                </Typography>
                <Typography variant="h5" fontWeight={700}>
                  {computeDuration(record?.checkInTime, record?.checkOutTime)}
                </Typography>
              </Paper>
            </Stack>

            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Button
                variant="contained"
                color="success"
                size="large"
                startIcon={<LoginIcon />}
                onClick={handleClockIn}
                disabled={acting || record?.isCheckedIn}
                fullWidth
              >
                {record?.isCheckedIn ? 'Clocked In' : 'Clock In'}
              </Button>
              <Button
                variant="contained"
                color="error"
                size="large"
                startIcon={<LogoutIcon />}
                onClick={handleClockOut}
                disabled={acting || !record?.isCheckedIn || record?.isCheckedOut}
                fullWidth
              >
                {record?.isCheckedOut ? 'Clocked Out' : 'Clock Out'}
              </Button>
            </Stack>
          </Stack>
        )}
      </Paper>
    </Box>
  );
}
