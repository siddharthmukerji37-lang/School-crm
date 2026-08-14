import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box, Button, Typography, Paper, TextField, Tabs, Tab, Table, TableBody,
  TableCell, TableContainer, TableHead, TableRow, Radio,
  CircularProgress, Stack, Divider, Chip,
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import axiosInstance from '../../services/axiosInstance';
import toast from 'react-hot-toast';

const STATUS_OPTIONS = ['Present', 'Absent', 'Late', 'Excused'];

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

export default function StaffAttendancePage() {
  const navigate = useNavigate();

  const [tab, setTab] = useState('Teacher');
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [people, setPeople] = useState([]);
  const [attendance, setAttendance] = useState({});
  const [times, setTimes] = useState({});
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const roleParam = tab === 'Teacher' ? 'Teacher' : 'Employee';

  useEffect(() => {
    setAttendance({});
    setTimes({});
    if (!date) {
      setPeople([]);
      return;
    }

    let cancelled = false;
    const load = async () => {
      setLoading(true);
      try {
        const listUrl = tab === 'Teacher' ? '/teachers' : '/employees';
        const [listRes, markedRes] = await Promise.all([
          axiosInstance.get(listUrl, { params: { pageSize: 100 } }),
          axiosInstance.get('/attendance/staff', {
            params: { date, role: roleParam, pageSize: 100 },
          }),
        ]);
        if (cancelled) return;

        const items = listRes.data.data?.items || listRes.data.data || [];
        setPeople(items);

        const initial = {};
        const timesMap = {};
        items.forEach((p) => { initial[p.id] = 'Present'; });

        const marked = markedRes.data.data?.items || [];
        marked.forEach((r) => {
          const id = r.teacherId || r.employeeId;
          if (id) {
            initial[id] = r.status;
            timesMap[id] = { checkInTime: r.checkInTime, checkOutTime: r.checkOutTime };
          }
        });

        setAttendance(initial);
        setTimes(timesMap);
      } catch {
        if (!cancelled) setPeople([]);
      } finally {
        if (!cancelled) setLoading(false);
      }
    };

    load();
    return () => { cancelled = true; };
  }, [tab, date, roleParam]);

  const handleStatusChange = (id, status) => {
    setAttendance((prev) => ({ ...prev, [id]: status }));
  };

  const markAllPresent = () => {
    const all = {};
    people.forEach((p) => { all[p.id] = 'Present'; });
    setAttendance(all);
  };

  const handleSubmit = async () => {
    if (!people.length) { toast.error('No records to mark'); return; }
    setSubmitting(true);
    try {
      const records = Object.entries(attendance).map(([id, status]) => ({
        ...(tab === 'Teacher' ? { teacherId: id } : { employeeId: id }),
        status,
        remarks: '',
      }));

      const url = tab === 'Teacher'
        ? '/attendance/staff/teachers/mark'
        : '/attendance/staff/employees/mark';

      await axiosInstance.post(url, { date, records });
      toast.success(`${tab} attendance marked successfully`);
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to mark attendance');
    } finally {
      setSubmitting(false);
    }
  };

  const getPersonName = (p) =>
    `${p.firstName || ''} ${p.lastName || ''}`.trim() || p.fullName || p.name || '—';

  const markedCount = people.filter((p) => attendance[p.id] === 'Present').length;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/attendance')} variant="outlined">
          Back
        </Button>
        <Typography variant="h4" fontWeight={700}>Staff Attendance</Typography>
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>Attendance Details</Typography>
        <Divider sx={{ mb: 3 }} />
        <Stack direction="column" spacing={2} alignItems="flex-start">
          <Tabs value={tab} onChange={(_, v) => setTab(v)} textColor="primary" indicatorColor="primary">
            <Tab label="Teachers" value="Teacher" />
            <Tab label="Employees" value="Employee" />
          </Tabs>
          <TextField
            size="small" label="Date" type="date" value={date}
            onChange={(e) => setDate(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
          />
          {people.length > 0 && (
            <Chip
              label={`${markedCount}/${people.length} present`}
              size="small"
              color="success"
              variant="outlined"
            />
          )}
          <Button variant="outlined" onClick={markAllPresent} disabled={!people.length}>
            Mark All Present
          </Button>
        </Stack>
      </Paper>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : people.length > 0 ? (
        <Paper>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>#</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Name</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>{tab === 'Teacher' ? 'Department' : 'Department'}</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Clock In</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Clock Out</TableCell>
                  {STATUS_OPTIONS.map((s) => (
                    <TableCell key={s} align="center" sx={{ fontWeight: 700 }}>{s}</TableCell>
                  ))}
                </TableRow>
              </TableHead>
              <TableBody>
                {people.map((person, idx) => (
                  <TableRow key={person.id}>
                    <TableCell>{idx + 1}</TableCell>
                    <TableCell>{getPersonName(person)}</TableCell>
                    <TableCell>{person.departmentName || person.designationName || '—'}</TableCell>
                    <TableCell sx={{ whiteSpace: 'nowrap' }}>
                      {times[person.id]?.checkInTime ? formatTime(times[person.id].checkInTime) : '—'}
                    </TableCell>
                    <TableCell sx={{ whiteSpace: 'nowrap' }}>
                      {times[person.id]?.checkOutTime ? formatTime(times[person.id].checkOutTime) : '—'}
                    </TableCell>
                    {STATUS_OPTIONS.map((s) => (
                      <TableCell key={s} align="center">
                        <Radio
                          checked={attendance[person.id] === s}
                          onChange={() => handleStatusChange(person.id, s)}
                          size="small"
                          color={s === 'Present' ? 'success' : s === 'Absent' ? 'error' : 'primary'}
                        />
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
          <Box sx={{ p: 2, display: 'flex', justifyContent: 'flex-end' }}>
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSubmit}
              disabled={submitting}
            >
              {submitting ? 'Saving...' : 'Save Attendance'}
            </Button>
          </Box>
        </Paper>
      ) : (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography color="text.secondary">
            No {tab.toLowerCase()} records found for the selected date.
          </Typography>
        </Paper>
      )}
    </Box>
  );
}
