import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
import {
  Box, Button, Typography, Paper, TextField, MenuItem, Table, TableBody,
  TableCell, TableContainer, TableHead, TableRow, Radio, RadioGroup,
  FormControlLabel, CircularProgress, Stack, Divider,
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { markAttendance } from '../../store/slices/attendanceSlice';
import axiosInstance from '../../services/axiosInstance';
import toast from 'react-hot-toast';

const STATUS_OPTIONS = ['Present', 'Absent', 'Late', 'Excused'];

export default function AttendanceMarkPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [classes, setClasses] = useState([]);
  const [sections, setSections] = useState([]);
  const [selectedClass, setSelectedClass] = useState('');
  const [selectedSection, setSelectedSection] = useState('');
  const [students, setStudents] = useState([]);
  const [attendance, setAttendance] = useState({});
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const fetchClasses = async () => {
      try {
        const res = await axiosInstance.get('/schools');
        const schools = res.data.data?.items || res.data.data || [];
        if (schools.length > 0) {
          const classRes = await axiosInstance.get(`/schools/${schools[0].id}/classes`);
          setClasses(classRes.data.data || []);
        }
      } catch { setClasses([]); }
    };
    fetchClasses();
  }, []);

  useEffect(() => {
    if (!selectedClass) { setSections([]); return; }
    const fetchSections = async () => {
      try {
        const res = await axiosInstance.get(`/schools/classes/${selectedClass}/sections`);
        setSections(res.data.data || []);
      } catch { setSections([]); }
    };
    fetchSections();
  }, [selectedClass]);

  useEffect(() => {
    if (!selectedClass || !selectedSection) { setStudents([]); return; }
    const fetchStudents = async () => {
      setLoading(true);
      try {
        const res = await axiosInstance.get(`/students?classRoomId=${selectedClass}&sectionId=${selectedSection}&pageSize=100`);
        const items = res.data.data?.items || [];
        setStudents(items);
        const initial = {};
        items.forEach((s) => { initial[s.id] = 'Present'; });
        setAttendance(initial);
      } catch { setStudents([]); }
      finally { setLoading(false); }
    };
    fetchStudents();
  }, [selectedClass, selectedSection]);

  const handleStatusChange = (studentId, status) => {
    setAttendance((prev) => ({ ...prev, [studentId]: status }));
  };

  const markAllPresent = () => {
    const all = {};
    students.forEach((s) => { all[s.id] = 'Present'; });
    setAttendance(all);
  };

  const handleSubmit = async () => {
    if (!students.length) { toast.error('No students to mark'); return; }
    setSubmitting(true);
    try {
      const records = Object.entries(attendance).map(([studentId, status]) => ({
        studentId,
        status,
        remarks: '',
      }));
      const result = await dispatch(markAttendance({
        date,
        classRoomId: selectedClass,
        sectionId: selectedSection,
        records,
      }));
      if (markAttendance.fulfilled.match(result)) {
        toast.success('Attendance marked successfully');
        navigate('/attendance');
      } else {
        toast.error(result.payload || 'Failed to mark attendance');
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/attendance')} variant="outlined">
          Back
        </Button>
        <Typography variant="h4" fontWeight={700}>Mark Attendance</Typography>
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>Attendance Details</Typography>
        <Divider sx={{ mb: 3 }} />
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <TextField
            size="small" label="Date" type="date" value={date}
            onChange={(e) => setDate(e.target.value)}
            slotProps={{ inputLabel: { shrink: true } }}
          />
          <TextField
            select size="small" label="Class" value={selectedClass}
            onChange={(e) => { setSelectedClass(e.target.value); setSelectedSection(''); setStudents([]); }}
            sx={{ minWidth: 160 }}
          >
            {classes.map((c) => (
              <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>
            ))}
          </TextField>
          <TextField
            select size="small" label="Section" value={selectedSection}
            onChange={(e) => setSelectedSection(e.target.value)}
            disabled={!selectedClass}
            sx={{ minWidth: 160 }}
          >
            {sections.map((s) => (
              <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>
            ))}
          </TextField>
          <Button variant="outlined" onClick={markAllPresent} disabled={!students.length}>
            Mark All Present
          </Button>
        </Stack>
      </Paper>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : students.length > 0 ? (
        <Paper>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>#</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Student Name</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Adm. No.</TableCell>
                  {STATUS_OPTIONS.map((s) => (
                    <TableCell key={s} align="center" sx={{ fontWeight: 700 }}>{s}</TableCell>
                  ))}
                </TableRow>
              </TableHead>
              <TableBody>
                {students.map((student, idx) => (
                  <TableRow key={student.id}>
                    <TableCell>{idx + 1}</TableCell>
                    <TableCell>{student.fullName || `${student.firstName} ${student.lastName}`}</TableCell>
                    <TableCell>{student.admissionNumber}</TableCell>
                    {STATUS_OPTIONS.map((s) => (
                      <TableCell key={s} align="center">
                        <Radio
                          checked={attendance[student.id] === s}
                          onChange={() => handleStatusChange(student.id, s)}
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
      ) : selectedClass && selectedSection ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography color="text.secondary">No students found for the selected class and section.</Typography>
        </Paper>
      ) : (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography color="text.secondary">Please select a class and section to mark attendance.</Typography>
        </Paper>
      )}
    </Box>
  );
}
