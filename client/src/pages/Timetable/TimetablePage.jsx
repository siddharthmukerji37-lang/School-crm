import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import {
  Box,
  Paper,
  Typography,
  Button,
  TextField,
  MenuItem,
  Tabs,
  Tab,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  TableContainer,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  IconButton,
  Chip,
  Stack,
  CircularProgress,
  FormControl,
  InputLabel,
  Select,
  Avatar,
  Alert,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import SaveIcon from '@mui/icons-material/Save';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import ReportProblemIcon from '@mui/icons-material/ReportProblem';
import WbSunnyIcon from '@mui/icons-material/WbSunny';
import axiosInstance from '../../services/axiosInstance';
import PageHeader from '../../components/common/PageHeader';
import toast from 'react-hot-toast';

const DAYS = [
  { label: 'Monday', dayOfWeek: 1 },
  { label: 'Tuesday', dayOfWeek: 2 },
  { label: 'Wednesday', dayOfWeek: 3 },
  { label: 'Thursday', dayOfWeek: 4 },
  { label: 'Friday', dayOfWeek: 5 },
  { label: 'Saturday', dayOfWeek: 6 },
];

const EMPTY_ID = '00000000-0000-0000-0000-000000000000';

function formatTime(value) {
  return value ? value.slice(0, 5) : '';
}

export default function TimetablePage() {
  const { user } = useSelector((state) => state.auth);
  const roles = user?.roles || [];
  const isAdmin = roles.some((r) => r === 'SuperAdmin' || r === 'Admin');
  const isTeacher = roles.some((r) => r === 'Teacher' || r === 'ClassTeacher');
  const isStudent = roles.some((r) => r === 'Student');

  const [schoolId, setSchoolId] = useState('');
  const [classes, setClasses] = useState([]);
  const [classId, setClassId] = useState('');
  const [sections, setSections] = useState([]);
  const [sectionId, setSectionId] = useState('');
  const [subjects, setSubjects] = useState([]);
  const [teachers, setTeachers] = useState([]);
  const [activeDay, setActiveDay] = useState(0);
  const [entriesByDay, setEntriesByDay] = useState({});
  const [loadingClasses, setLoadingClasses] = useState(true);
  const [loadingSections, setLoadingSections] = useState(false);
  const [loadingTimetable, setLoadingTimetable] = useState(false);
  const [saving, setSaving] = useState(false);

  const [myTimetable, setMyTimetable] = useState([]);
  const [loadingMy, setLoadingMy] = useState(false);

  const [mySectionTimetable, setMySectionTimetable] = useState([]);
  const [loadingMySection, setLoadingMySection] = useState(false);

  const [requestOpen, setRequestOpen] = useState(false);
  const [requestEntry, setRequestEntry] = useState(null);
  const [requestMessage, setRequestMessage] = useState('');
  const [sendingRequest, setSendingRequest] = useState(false);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState({});

  const dayEntries = entriesByDay[DAYS[activeDay].label] || [];

  useEffect(() => {
    if (isTeacher && !isAdmin) {
      const loadMy = async () => {
        setLoadingMy(true);
        try {
          const res = await axiosInstance.get('/schools/timetable/my');
          setMyTimetable(res.data.data || []);
        } catch {
          setMyTimetable([]);
          toast.error('Failed to load your timetable');
        } finally {
          setLoadingMy(false);
        }
      };
      loadMy();
    }
  }, [isTeacher, isAdmin]);

  useEffect(() => {
    if (isStudent && !isAdmin && !isTeacher) {
      const loadMySection = async () => {
        setLoadingMySection(true);
        try {
          const res = await axiosInstance.get('/schools/timetable/my-section');
          setMySectionTimetable(res.data.data || []);
        } catch {
          setMySectionTimetable([]);
          toast.error('Failed to load your class timetable');
        } finally {
          setLoadingMySection(false);
        }
      };
      loadMySection();
    }
  }, [isStudent, isAdmin, isTeacher]);

  useEffect(() => {
    const init = async () => {
      try {
        const schoolRes = await axiosInstance.get('/schools');
        const items = schoolRes.data.data?.items || schoolRes.data.data || [];
        const school = Array.isArray(items) ? items[0] : null;
        if (!school?.id) return;
        setSchoolId(school.id);

        const classRes = await axiosInstance.get(`/schools/${school.id}/classes`);
        const classList = classRes.data.data || [];
        setClasses(classList);

        const subjectRes = await axiosInstance.get(`/schools/${school.id}/subjects`);
        setSubjects(Array.isArray(subjectRes.data.data) ? subjectRes.data.data : []);

        const teacherRes = await axiosInstance.get('/teachers', { params: { pageSize: 100 } });
        setTeachers(teacherRes.data.data?.items || []);
      } catch {
        toast.error('Failed to load timetable data');
      } finally {
        setLoadingClasses(false);
      }
    };
    init();
  }, []);

  useEffect(() => {
    if (!classId) return;
    const loadSections = async () => {
      setLoadingSections(true);
      setSectionId('');
      setEntriesByDay({});
      try {
        const res = await axiosInstance.get(`/schools/classes/${classId}/sections`);
        setSections(res.data.data || []);
      } catch {
        setSections([]);
        toast.error('Failed to load sections');
      } finally {
        setLoadingSections(false);
      }
    };
    loadSections();
  }, [classId]);

  useEffect(() => {
    if (!sectionId) return;
    const loadTimetable = async () => {
      setLoadingTimetable(true);
      const today = new Date();
      const mondayOffset = (today.getDay() + 6) % 7;
      const monday = new Date(today);
      monday.setDate(today.getDate() - mondayOffset);

      try {
        const result = {};
        for (let i = 0; i < DAYS.length; i++) {
          const date = new Date(monday);
          date.setDate(monday.getDate() + i);
          const dateStr = date.toISOString().split('T')[0];
          const res = await axiosInstance.get(
            `/schools/sections/${sectionId}/timetable`,
            { params: { date: dateStr } }
          );
          result[DAYS[i].label] = (res.data.data || []).map((e) => ({
            id: e.id,
            sectionId: e.sectionId,
            subjectId: e.subjectId,
            subjectName: e.subjectName,
            teacherId: e.teacherId || null,
            teacherName: e.teacherName || '',
            startTime: formatTime(e.startTime),
            endTime: formatTime(e.endTime),
          }));
        }
        setEntriesByDay(result);
      } catch {
        toast.error('Failed to load timetable');
      } finally {
        setLoadingTimetable(false);
      }
    };
    loadTimetable();
  }, [sectionId]);

  const classSubjects = subjects.filter((s) => s.classRoomId === classId);
  const availableSubjects = classSubjects.length > 0 ? classSubjects : subjects;

  const teacherName = (t) =>
    t ? `${t.firstName || ''} ${t.lastName || ''}`.trim() : '';

  const openAdd = () => {
    setEditing(null);
    setForm({
      dayOfWeek: DAYS[activeDay].dayOfWeek,
      startTime: '09:00',
      endTime: '09:45',
      subjectId: '',
      teacherId: '',
    });
    setDialogOpen(true);
  };

  const openEdit = (entry) => {
    setEditing(entry);
    setForm({
      id: entry.id,
      dayOfWeek: DAYS[activeDay].dayOfWeek,
      startTime: entry.startTime,
      endTime: entry.endTime,
      subjectId: entry.subjectId,
      teacherId: entry.teacherId || '',
    });
    setDialogOpen(true);
  };

  const handleDialogClose = () => setDialogOpen(false);

  const handleSaveEntry = () => {
    const dayLabel = DAYS[activeDay].label;
    if (!form.subjectId || !form.startTime || !form.endTime) {
      toast.error('Please fill subject and timings');
      return;
    }
    if (form.startTime >= form.endTime) {
      toast.error('End time must be after start time');
      return;
    }

    const newEntry = {
      id: editing?.id || EMPTY_ID,
      sectionId,
      subjectId: form.subjectId,
      subjectName: availableSubjects.find((s) => s.id === form.subjectId)?.name || '',
      teacherId: form.teacherId || null,
      teacherName: teacherName(teachers.find((t) => t.id === form.teacherId)),
      startTime: form.startTime,
      endTime: form.endTime,
    };

    setEntriesByDay((prev) => {
      const current = prev[dayLabel] || [];
      const exists = editing
        ? current.some((e) => e.id === editing.id)
        : false;
      return {
        ...prev,
        [dayLabel]: exists
          ? current.map((e) => (e.id === editing.id ? newEntry : e))
          : [...current, newEntry],
      };
    });

    setDialogOpen(false);
  };

  const handleDeleteEntry = (id) => {
    const dayLabel = DAYS[activeDay].label;
    setEntriesByDay((prev) => ({
      ...prev,
      [dayLabel]: (prev[dayLabel] || []).filter((e) => e.id !== id),
    }));
  };

  const handleSave = async () => {
    if (!sectionId) return;
    const payload = DAYS.flatMap((day) =>
      (entriesByDay[day.label] || []).map((e) => ({
        id: e.id,
        sectionId,
        subjectId: e.subjectId,
        teacherId: e.teacherId,
        dayOfWeek: day.dayOfWeek,
        startTime: e.startTime,
        endTime: e.endTime,
      }))
    );

    setSaving(true);
    try {
      await axiosInstance.post(`/schools/sections/${sectionId}/timetable`, payload);
      toast.success('Timetable saved');
    } catch {
      toast.error('Failed to save timetable');
    } finally {
      setSaving(false);
    }
  };

  const openRequestChange = (entry) => {
    setRequestEntry(entry);
    setRequestMessage('');
    setRequestOpen(true);
  };

  const sendChangeRequest = async () => {
    if (!requestMessage.trim()) {
      toast.error('Please describe the change you need');
      return;
    }
    setSendingRequest(true);
    try {
      const detail = requestEntry
        ? `${requestEntry.className || ''} • Section ${requestEntry.sectionName || ''} • ${requestEntry.subjectName || ''} • ${requestEntry.dayLabel} ${formatTime(requestEntry.startTime)}-${formatTime(requestEntry.endTime)}`
        : 'Timetable';
      await axiosInstance.post('/notifications/timetable-change', {
        message: `${detail}\nReason: ${requestMessage.trim()}`,
      });
      toast.success('Change request sent to admin');
      setRequestOpen(false);
    } catch {
      toast.error('Failed to send request');
    } finally {
      setSendingRequest(false);
    }
  };

  const dayOfWeekLabel = (d) => {
    const day = DAYS.find((x) => x.dayOfWeek === d);
    return day ? day.label : '';
  };

  const renderTodayPanel = (entries, showTeacher = false) => {
    const todayDayOfWeek = new Date().getDay();
    const todays = entries
      .filter((e) => e.dayOfWeek === todayDayOfWeek)
      .sort((a, b) => (a.startTime || '').localeCompare(b.startTime || ''));

    return (
      <Paper sx={{ p: 2.5, borderRadius: 2, mb: 3 }}>
        <Typography
          variant="subtitle1"
          fontWeight={600}
          sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}
        >
          <WbSunnyIcon color="warning" fontSize="small" />
          Today's Classes
        </Typography>
        {todayDayOfWeek === 0 || todays.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            No classes scheduled for today.
          </Typography>
        ) : (
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} flexWrap="wrap" useFlexGap>
            {todays.map((e) => (
              <Chip
                key={e.id}
                icon={<AccessTimeIcon fontSize="small" />}
                label={
                  showTeacher
                    ? `${e.subjectName || 'Subject'} • ${e.teacherName || 'Not assigned'} • ${formatTime(e.startTime)}–${formatTime(e.endTime)}`
                    : `${e.subjectName || 'Subject'} • ${e.className || ''} Sec ${e.sectionName || ''} • ${formatTime(e.startTime)}–${formatTime(e.endTime)}`
                }
                variant="outlined"
                sx={{ borderRadius: 2 }}
              />
            ))}
          </Stack>
        )}
      </Paper>
    );
  };

  const renderTeacherView = () => {
    const dayRows = DAYS.map((day) => ({
      ...day,
      entries: myTimetable
        .filter((e) => e.dayOfWeek === day.dayOfWeek)
        .sort((a, b) => (a.startTime || '').localeCompare(b.startTime || '')),
    }));

    const periods = [];
    dayRows.forEach(({ entries }) => {
      entries.forEach((e) => {
        const key = `${formatTime(e.startTime)}-${formatTime(e.endTime)}`;
        if (!periods.some((p) => p.key === key)) {
          periods.push({ key, startTime: e.startTime, endTime: e.endTime, dayLabel: e.dayLabel });
        }
      });
    });
    periods.sort((a, b) => (a.startTime || '').localeCompare(b.startTime || ''));

    return (
      <Box>
        {renderTodayPanel(myTimetable, false)}
        <Alert severity="info" sx={{ mb: 3 }}>
          This is your weekly routine. If you need any change, use <strong>Request Change</strong> on a period —
          the admin will review and update it.
        </Alert>
        {loadingMy ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
        ) : periods.length === 0 ? (
          <Paper sx={{ p: 4, textAlign: 'center' }}>
            <Typography color="text.secondary">
              No periods are assigned to you yet. Contact the admin if you believe this is a mistake.
            </Typography>
          </Paper>
        ) : (
          <Paper sx={{ borderRadius: 2, overflow: 'hidden' }}>
            <TableContainer>
              <Table sx={{ minWidth: 700 }}>
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Period</TableCell>
                    {DAYS.map((day) => (
                      <TableCell key={day.dayOfWeek} sx={{ fontWeight: 600 }}>{day.label}</TableCell>
                    ))}
                    <TableCell sx={{ fontWeight: 600 }}>Change</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {periods.map((period, idx) => {
                    const cellEntry = (dayOfWeek) => {
                      const row = dayRows.find((d) => d.dayOfWeek === dayOfWeek);
                      return (row?.entries || []).find(
                        (e) => formatTime(e.startTime) === formatTime(period.startTime) &&
                              formatTime(e.endTime) === formatTime(period.endTime)
                      );
                    };
                    const rowEntries = DAYS.map((d) => cellEntry(d.dayOfWeek)).filter(Boolean);
                    const firstEntry = rowEntries[0];
                    return (
                      <TableRow key={period.key} hover>
                        <TableCell>
                          <Stack spacing={0.5}>
                            <Typography variant="body2" fontWeight={600}>P{idx + 1}</Typography>
                            <Typography variant="caption" color="text.secondary">
                              {formatTime(period.startTime)} – {formatTime(period.endTime)}
                            </Typography>
                          </Stack>
                        </TableCell>
                        {DAYS.map((day) => {
                          const entry = cellEntry(day.dayOfWeek);
                          return (
                            <TableCell key={day.dayOfWeek}>
                              {entry ? (
                                <Stack spacing={0.5}>
                                  <Chip
                                    label={entry.subjectName || 'Subject'}
                                    size="small" color="primary" variant="outlined"
                                  />
                                  <Typography variant="caption" color="text.secondary">
                                    {entry.teacherName || 'Not assigned'}
                                  </Typography>
                                  <Typography variant="caption" color="text.disabled">
                                    {entry.className} • Sec {entry.sectionName}
                                  </Typography>
                                </Stack>
                              ) : (
                                <Typography variant="caption" color="text.disabled">—</Typography>
                              )}
                            </TableCell>
                          );
                        })}
                        <TableCell>
                          <Button
                            size="small"
                            variant="outlined"
                            color="warning"
                            startIcon={<ReportProblemIcon fontSize="small" />}
                            onClick={() =>
                              openRequestChange({
                                className: firstEntry?.className || '',
                                sectionName: firstEntry?.sectionName || '',
                                subjectName: firstEntry?.subjectName || '',
                                dayLabel: dayOfWeekLabel(firstEntry?.dayOfWeek),
                                startTime: period.startTime,
                                endTime: period.endTime,
                              })
                            }
                          >
                            Request Change
                          </Button>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        )}
      </Box>
    );
  };

  const renderStudentView = () => {
    const dayRows = DAYS.map((day) => ({
      ...day,
      entries: mySectionTimetable
        .filter((e) => e.dayOfWeek === day.dayOfWeek)
        .sort((a, b) => (a.startTime || '').localeCompare(b.startTime || '')),
    }));

    const periods = [];
    dayRows.forEach(({ entries }) => {
      entries.forEach((e) => {
        const key = `${formatTime(e.startTime)}-${formatTime(e.endTime)}`;
        if (!periods.some((p) => p.key === key)) {
          periods.push({ key, startTime: e.startTime, endTime: e.endTime });
        }
      });
    });
    periods.sort((a, b) => (a.startTime || '').localeCompare(b.startTime || ''));

    return (
      <Box>
        {renderTodayPanel(mySectionTimetable, true)}
        {loadingMySection ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}><CircularProgress /></Box>
        ) : periods.length === 0 ? (
          <Paper sx={{ p: 4, textAlign: 'center' }}>
            <Typography color="text.secondary">
              No timetable has been set for your class yet.
            </Typography>
          </Paper>
        ) : (
          <Paper sx={{ borderRadius: 2, overflow: 'hidden' }}>
            <TableContainer>
              <Table sx={{ minWidth: 700 }}>
                <TableHead>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600 }}>Period</TableCell>
                    {DAYS.map((day) => (
                      <TableCell key={day.dayOfWeek} sx={{ fontWeight: 600 }}>{day.label}</TableCell>
                    ))}
                  </TableRow>
                </TableHead>
                <TableBody>
                  {periods.map((period, idx) => {
                    const cellEntry = (dayOfWeek) => {
                      const row = dayRows.find((d) => d.dayOfWeek === dayOfWeek);
                      return (row?.entries || []).find(
                        (e) => formatTime(e.startTime) === formatTime(period.startTime) &&
                              formatTime(e.endTime) === formatTime(period.endTime)
                      );
                    };
                    return (
                      <TableRow key={period.key} hover>
                        <TableCell>
                          <Stack spacing={0.5}>
                            <Typography variant="body2" fontWeight={600}>P{idx + 1}</Typography>
                            <Typography variant="caption" color="text.secondary">
                              {formatTime(period.startTime)} – {formatTime(period.endTime)}
                            </Typography>
                          </Stack>
                        </TableCell>
                        {DAYS.map((day) => {
                          const entry = cellEntry(day.dayOfWeek);
                          return (
                            <TableCell key={day.dayOfWeek}>
                              {entry ? (
                                <Stack spacing={0.5}>
                                  <Chip
                                    label={entry.subjectName || 'Subject'}
                                    size="small" color="primary" variant="outlined"
                                  />
                                  <Typography variant="caption" color="text.secondary">
                                    {entry.teacherName || 'Not assigned'}
                                  </Typography>
                                </Stack>
                              ) : (
                                <Typography variant="caption" color="text.disabled">—</Typography>
                              )}
                            </TableCell>
                          );
                        })}
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        )}
      </Box>
    );
  };

  if (isTeacher && !isAdmin) {
    return (
      <Box>
        <PageHeader
          title="My Routine"
          subtitle="Your weekly classes across all sections. Request changes and the admin will update the timetable."
        />
        {renderTeacherView()}
        <Dialog open={requestOpen} onClose={() => setRequestOpen(false)} fullWidth maxWidth="sm">
          <DialogTitle>Request Timetable Change</DialogTitle>
          <DialogContent>
            {requestEntry?.subjectName && (
              <Typography
                variant="body2"
                sx={{ mb: 2, p: 1.5, bgcolor: 'grey.100', borderRadius: 1 }}
              >
                {requestEntry.dayLabel} • {requestEntry.className} • Section {requestEntry.sectionName}
                <br />
                {requestEntry.subjectName} • {formatTime(requestEntry.startTime)}–
                {formatTime(requestEntry.endTime)}
              </Typography>
            )}
            <TextField
              label="What change do you need?"
              multiline
              rows={4}
              fullWidth
              value={requestMessage}
              onChange={(e) => setRequestMessage(e.target.value)}
              placeholder="e.g. Please move this period to Tuesday morning, or swap it with another class..."
            />
          </DialogContent>
          <DialogActions sx={{ px: 3, pb: 2 }}>
            <Button onClick={() => setRequestOpen(false)}>Cancel</Button>
            <Button
              variant="contained"
              color="warning"
              startIcon={<ReportProblemIcon />}
              onClick={sendChangeRequest}
              disabled={sendingRequest}
            >
              {sendingRequest ? 'Sending...' : 'Send to Admin'}
            </Button>
          </DialogActions>
        </Dialog>
      </Box>
    );
  }

  if (isStudent && !isAdmin && !isTeacher) {
    return (
      <Box>
        <PageHeader
          title="Class Timetable"
          subtitle="Your class schedule across the week."
        />
        {renderStudentView()}
      </Box>
    );
  }

  return (
    <Box>
      <PageHeader
        title={isTeacher && !isAdmin ? 'My Routine' : 'Timetable'}
        subtitle={isTeacher && !isAdmin
          ? 'Your weekly classes across all sections.'
          : 'Plan which teacher takes which subject for each class section and time slot.'}
        actions={
          isAdmin && sectionId ? (
            <Button
              variant="contained"
              startIcon={<SaveIcon />}
              onClick={handleSave}
              disabled={saving}
            >
              {saving ? 'Saving...' : 'Save Timetable'}
            </Button>
          ) : null
        }
      />

      <Paper sx={{ p: 3, borderRadius: 2, mb: 3 }}>
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
          <FormControl fullWidth>
            <InputLabel>Class</InputLabel>
            <Select
              value={classId}
              onChange={(e) => setClassId(e.target.value)}
              label="Class"
            >
              {classes.map((c) => (
                <MenuItem key={c.id} value={c.id}>
                  {c.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
          <FormControl fullWidth>
            <InputLabel>Section</InputLabel>
            <Select
              value={sectionId}
              onChange={(e) => setSectionId(e.target.value)}
              label="Section"
              disabled={loadingSections || !classId}
            >
              {sections.map((s) => (
                <MenuItem key={s.id} value={s.id}>
                  {s.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Stack>
      </Paper>

      {loadingClasses ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
          <CircularProgress />
        </Box>
      ) : !sectionId ? (
        <Paper sx={{ p: 4, borderRadius: 2, textAlign: 'center' }}>
          <Typography color="text.secondary">
            Select a class and section to view or plan its timetable.
          </Typography>
        </Paper>
      ) : (
        <Paper sx={{ borderRadius: 2, overflow: 'hidden' }}>
          <Tabs
            value={activeDay}
            onChange={(_, v) => setActiveDay(v)}
            variant="scrollable"
            scrollButtons="auto"
            sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}
          >
            {DAYS.map((day) => (
              <Tab key={day.label} label={day.label} />
            ))}
          </Tabs>

          <Box sx={{ p: 3 }}>
            <Stack
              direction="row"
              justifyContent="space-between"
              alignItems="center"
              sx={{ mb: 2 }}
            >
              <Typography variant="subtitle1" fontWeight={600} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <AccessTimeIcon color="primary" fontSize="small" />
                {DAYS[activeDay].label}
              </Typography>
              {isAdmin && (
                <Button variant="contained" startIcon={<AddIcon />} onClick={openAdd}>
                  Add Period
                </Button>
              )}
            </Stack>

            {loadingTimetable ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
                <CircularProgress />
              </Box>
            ) : dayEntries.length === 0 ? (
              <Box sx={{ py: 5, textAlign: 'center' }}>
                <Typography color="text.secondary" gutterBottom>
                  No periods scheduled for {DAYS[activeDay].label}.
                </Typography>
                {isAdmin && (
                  <Button variant="outlined" startIcon={<AddIcon />} onClick={openAdd} sx={{ mt: 1 }}>
                    Add First Period
                  </Button>
                )}
              </Box>
            ) : (
              <TableContainer>
                <Table sx={{ minWidth: 600 }}>
                  <TableHead>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 600 }}>Period</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Start</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>End</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Subject</TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>Teacher</TableCell>
                      {isAdmin && <TableCell sx={{ fontWeight: 600 }} align="right">Actions</TableCell>}
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {[...dayEntries]
                      .sort((a, b) => a.startTime.localeCompare(b.startTime))
                      .map((entry, index) => (
                        <TableRow key={entry.id + index} hover>
                          <TableCell>{index + 1}</TableCell>
                          <TableCell>{entry.startTime}</TableCell>
                          <TableCell>{entry.endTime}</TableCell>
                          <TableCell>
                            <Chip
                              label={entry.subjectName || 'Subject'}
                              size="small"
                              variant="outlined"
                              color="primary"
                            />
                          </TableCell>
                          <TableCell>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <Avatar sx={{ width: 28, height: 28, fontSize: '0.75rem', bgcolor: 'secondary.light', color: 'secondary.main' }}>
                                {entry.teacherName
                                  .split(' ')
                                  .map((n) => n[0])
                                  .slice(0, 2)
                                  .join('') || '?'}
                              </Avatar>
                              <Typography variant="body2">
                                {entry.teacherName || 'Not assigned'}
                              </Typography>
                            </Box>
                          </TableCell>
                          {isAdmin && (
                            <TableCell align="right">
                              <IconButton size="small" onClick={() => openEdit(entry)}>
                                <EditIcon fontSize="small" />
                              </IconButton>
                              <IconButton size="small" color="error" onClick={() => handleDeleteEntry(entry.id)}>
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </TableCell>
                          )}
                        </TableRow>
                      ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}

            {isAdmin && !loadingTimetable && dayEntries.length > 0 && (
              <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3 }}>
                <Button
                  variant="contained"
                  startIcon={<SaveIcon />}
                  onClick={handleSave}
                  disabled={saving}
                >
                  {saving ? 'Saving...' : 'Save Timetable'}
                </Button>
              </Box>
            )}
          </Box>
        </Paper>
      )}

      <Dialog open={dialogOpen} onClose={handleDialogClose} fullWidth maxWidth="sm">
        <DialogTitle>
          {editing ? 'Edit Period' : 'Add Period'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Start Time"
              type="time"
              value={form.startTime || ''}
              onChange={(e) => setForm({ ...form, startTime: e.target.value })}
              InputLabelProps={{ shrink: true }}
              fullWidth
            />
            <TextField
              label="End Time"
              type="time"
              value={form.endTime || ''}
              onChange={(e) => setForm({ ...form, endTime: e.target.value })}
              InputLabelProps={{ shrink: true }}
              fullWidth
            />
            <FormControl fullWidth>
              <InputLabel>Subject</InputLabel>
              <Select
                value={form.subjectId || ''}
                onChange={(e) => setForm({ ...form, subjectId: e.target.value })}
                label="Subject"
              >
                {availableSubjects.map((s) => (
                  <MenuItem key={s.id} value={s.id}>
                    {s.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <FormControl fullWidth>
              <InputLabel>Teacher</InputLabel>
              <Select
                value={form.teacherId || ''}
                onChange={(e) => setForm({ ...form, teacherId: e.target.value })}
                label="Teacher"
              >
                {teachers.map((t) => (
                  <MenuItem key={t.id} value={t.id}>
                    {teacherName(t)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleDialogClose}>Cancel</Button>
          <Button variant="contained" onClick={handleSaveEntry}>
            {editing ? 'Update' : 'Add'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
