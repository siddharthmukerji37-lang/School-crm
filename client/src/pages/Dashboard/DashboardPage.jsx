import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box,
  Grid,
  Paper,
  Typography,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Avatar,
  Skeleton,
  Divider,
  Chip,
  LinearProgress,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
} from '@mui/material';
import SchoolIcon from '@mui/icons-material/School';
import PeopleIcon from '@mui/icons-material/People';
import BadgeIcon from '@mui/icons-material/Badge';
import ClassIcon from '@mui/icons-material/Class';
import EventAvailableIcon from '@mui/icons-material/EventAvailable';
import PaymentsIcon from '@mui/icons-material/Payments';
import PendingActionsIcon from '@mui/icons-material/PendingActions';
import QuizIcon from '@mui/icons-material/Quiz';
import CakeIcon from '@mui/icons-material/Cake';
import CampaignIcon from '@mui/icons-material/Campaign';
import { Line, Doughnut, Bar } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  BarElement,
  Title,
  Tooltip as ChartTooltip,
  Legend,
  Filler,
} from 'chart.js';
import StatsCard from '../../components/common/StatsCard';
import PageHeader from '../../components/common/PageHeader';
import {
  fetchDashboardStats,
  fetchAttendanceChart,
} from '../../store/slices/dashboardSlice';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
  BarElement,
  Title,
  ChartTooltip,
  Legend,
  Filler
);

const CHART_OPTIONS = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom',
      labels: { padding: 16, usePointStyle: true, pointStyle: 'circle' },
    },
  },
  scales: {
    x: {
      grid: { display: false },
      ticks: { font: { size: 12 } },
    },
    y: {
      grid: { color: 'rgba(0,0,0,0.05)' },
      ticks: { font: { size: 12 } },
    },
  },
};

export default function DashboardPage() {
  const dispatch = useDispatch();
  const [selectedExamId, setSelectedExamId] = useState('');
  const { stats, attendanceChart, loading } = useSelector(
    (state) => state.dashboard
  );
  const { user } = useSelector((state) => state.auth);
  const roles = user?.roles || [];
  const isAdmin = roles.some(
    (r) => r === 'SuperAdmin' || r === 'Admin' || r === 'SchoolAdmin'
  );
  const canViewFees =
    isAdmin || roles.some((r) => r === 'Accountant');

  useEffect(() => {
    dispatch(fetchDashboardStats());
    dispatch(fetchAttendanceChart({ months: 6 }));
  }, [dispatch]);

  const statsData = stats || {};

  const attendance = Array.isArray(attendanceChart) ? attendanceChart : [];
  const attendanceChartData = {
    labels: attendance.length ? attendance.map((i) => i.label) : ['Mon', 'Tue', 'Wed', 'Thu', 'Fri'],
    datasets: [
      {
        label: 'Attendance %',
        data: attendance.length ? attendance.map((i) => Number(i.value)) : [0, 0, 0, 0, 0],
        borderColor: '#1565C0',
        backgroundColor: 'rgba(21, 101, 192, 0.1)',
        fill: true,
        tension: 0.4,
        pointRadius: 4,
        pointHoverRadius: 6,
      },
    ],
  };

  const fee = statsData.feesCollected || {};
  const feeChartData = {
    labels: ['Collected', 'Pending', 'Overdue'],
    datasets: [
      {
        data: [
          Number(fee.totalCollected || 0),
          Number(fee.totalPending || 0),
          Number(fee.overdueFees || 0),
        ],
        backgroundColor: ['#2E7D32', '#F57C00', '#D32F2F'],
        borderWidth: 0,
      },
    ],
  };

  const priorityIcons = {
    High: '🔴',
    Medium: '🟡',
    Low: '🔵',
  };
  const announcements = (statsData.latestAnnouncements || []).map((a) => ({
    id: a.id,
    title: a.title,
    date: a.createdAt ? new Date(a.createdAt).toLocaleDateString() : '',
    icon: priorityIcons[a.priority] || '📢',
  }));

  const birthdays = (statsData.todayBirthdays || []).map((b) => ({
    id: b.id,
    name: b.name,
    class: b.className || (b.type === 'Teacher' ? 'Teacher' : ''),
  }));

  const pendingFeeStudents = statsData.pendingFeeStudents || [];

  const staffAttendance = statsData.staffAttendance || {};
  const teachersPresent = Number(staffAttendance.teachersPresent || 0);
  const teachersMarked = Number(staffAttendance.teachersMarked || 0);
  const totalTeachers = Number(staffAttendance.totalTeachers || 0);
  const employeesPresent = Number(staffAttendance.employeesPresent || 0);
  const employeesMarked = Number(staffAttendance.employeesMarked || 0);
  const totalEmployees = Number(staffAttendance.totalEmployees || 0);

  const examResults = statsData.examResults || [];
  const examOptions = [
    ...new Map(examResults.map((r) => [r.examId, { id: r.examId, name: r.examName }])).values(),
  ];
  const activeExamId = selectedExamId || examOptions[0]?.id || '';
  const filteredExamResults = examResults.filter((r) => r.examId === activeExamId);
  const examChartData = {
    labels: filteredExamResults.map((r) =>
      r.sectionName ? `${r.className} - ${r.sectionName}` : r.className
    ),
    datasets: [
      {
        label: 'Passed',
        data: filteredExamResults.map((r) => Number(r.passedCount || 0)),
        backgroundColor: '#2E7D32',
        borderRadius: 4,
      },
      {
        label: 'Failed',
        data: filteredExamResults.map((r) => Number(r.failedCount || 0)),
        backgroundColor: '#D32F2F',
        borderRadius: 4,
      },
    ],
  };

  const statCards = [
    {
      icon: <SchoolIcon />,
      title: 'Total Students',
      value: Number(statsData.totalStudents || 0).toLocaleString(),
      trend: 'neutral',
      trendValue: '',
      color: 'primary',
    },
    {
      icon: <PeopleIcon />,
      title: 'Total Teachers',
      value: Number(statsData.totalTeachers || 0).toLocaleString(),
      trend: 'neutral',
      trendValue: '',
      color: 'secondary',
    },
    {
      icon: <BadgeIcon />,
      title: 'Total Staff',
      value: Number(statsData.totalStaff || 0).toLocaleString(),
      trend: 'neutral',
      trendValue: '',
      color: 'info',
    },
    {
      icon: <ClassIcon />,
      title: 'Total Classes',
      value: Number(statsData.totalClasses || 0).toLocaleString(),
      trend: 'neutral',
      trendValue: '',
      color: 'warning',
    },
    {
      icon: <EventAvailableIcon />,
      title: "Student Attendance Today",
      value: `${Number(statsData.todayAttendance?.attendancePercentage ?? 0).toFixed(2)}%`,
      trend: 'neutral',
      trendValue: '',
      color: 'success',
    },
    {
      icon: <PaymentsIcon />,
      title: 'Fees Collected',
      value: `$${Number(fee.totalCollected || 0).toLocaleString()}`,
      trend: 'neutral',
      trendValue: '',
      color: 'success',
      feeOnly: true,
    },
    {
      icon: <PendingActionsIcon />,
      title: 'Pending Fees',
      value: `$${Number(statsData.pendingFees || 0).toLocaleString()}`,
      trend: 'neutral',
      trendValue: '',
      color: 'warning',
      feeOnly: true,
    },
    {
      icon: <QuizIcon />,
      title: 'Upcoming Exams',
      value: Number(statsData.upcomingExams || 0).toLocaleString(),
      trend: 'neutral',
      trendValue: '',
      color: 'error',
    },
  ].filter((card) => (card.feeOnly ? canViewFees : true));

  return (
    <Box>
      <PageHeader
        title="Dashboard"
        subtitle="Welcome back! Here's what's happening today."
      />

      <Grid container spacing={3} sx={{ mb: 4 }}>
        {statCards.map((card, index) => (
          <Grid size={{ xs: 12, sm: 6, md: 3 }} key={index}>
            {loading ? (
              <Skeleton variant="rounded" height={140} sx={{ borderRadius: 3 }} />
            ) : (
              <StatsCard {...card} />
            )}
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: canViewFees ? 8 : 12 }}>
          <Paper sx={{ p: 3, mb: { xs: 3, md: 0 } }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Attendance Trend
            </Typography>
            <Box sx={{ height: 280 }}>
              <Line data={attendanceChartData} options={CHART_OPTIONS} />
            </Box>
          </Paper>
        </Grid>

        {canViewFees && (
        <Grid size={{ xs: 12, md: 4 }}>
          <Paper sx={{ p: 3, mb: { xs: 3, md: 0 }, height: '100%' }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>
              Fee Collection
            </Typography>
            <Box
              sx={{
                height: 220,
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
              }}
            >
              <Doughnut
                data={feeChartData}
                options={{
                  responsive: true,
                  maintainAspectRatio: false,
                  plugins: {
                    legend: {
                      position: 'bottom',
                      labels: { padding: 12, usePointStyle: true, pointStyle: 'circle' },
                    },
                  },
                  cutout: '65%',
                }}
              />
            </Box>
          </Paper>
        </Grid>
        )}

        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <CampaignIcon color="primary" />
              <Typography variant="h6" fontWeight={600}>
                Recent Announcements
              </Typography>
            </Box>
            <List disablePadding>
              {announcements.map((item, index) => (
                <React.Fragment key={item.id}>
                  <ListItem disablePadding sx={{ py: 1.5 }}>
                    <ListItemAvatar>
                      <Avatar
                        sx={{
                          bgcolor: 'primary.light',
                          width: 40,
                          height: 40,
                          fontSize: 20,
                        }}
                      >
                        {item.icon}
                      </Avatar>
                    </ListItemAvatar>
                    <ListItemText
                      primary={
                        <Typography variant="body1" fontWeight={500}>
                          {item.title}
                        </Typography>
                      }
                      secondary={item.date}
                    />
                  </ListItem>
                  {index < announcements.length - 1 && <Divider />}
                </React.Fragment>
              ))}
              {announcements.length === 0 && (
                <Typography variant="body2" color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
                  No recent announcements
                </Typography>
              )}
            </List>
          </Paper>
        </Grid>

        <Grid size={{ xs: 12, md: 6 }}>
          <Paper sx={{ p: 3, height: '100%' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <CakeIcon color="secondary" />
              <Typography variant="h6" fontWeight={600}>
                Today&apos;s Birthdays
              </Typography>
            </Box>
            <List disablePadding>
              {birthdays.map((student, index) => (
                <React.Fragment key={student.id}>
                  <ListItem disablePadding sx={{ py: 1.5 }}>
                    <ListItemAvatar>
                      <Avatar
                        sx={{
                          bgcolor: 'secondary.light',
                          color: 'white',
                          width: 40,
                          height: 40,
                          fontWeight: 600,
                          fontSize: '0.875rem',
                        }}
                      >
                        {student.name
                          .split(' ')
                          .map((n) => n[0])
                          .join('')}
                      </Avatar>
                    </ListItemAvatar>
                    <ListItemText
                      primary={
                        <Typography variant="body1" fontWeight={500}>
                          {student.name}
                        </Typography>
                      }
                      secondary={student.class}
                    />
                  </ListItem>
                  {index < birthdays.length - 1 && <Divider />}
                </React.Fragment>
              ))}
              {birthdays.length === 0 && (
                <Typography variant="body2" color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
                  No birthdays today
                </Typography>
              )}
            </List>
          </Paper>
        </Grid>
      </Grid>

      {isAdmin && (
        <Grid container spacing={3} sx={{ mt: 0.5 }}>
          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <EventAvailableIcon color="primary" />
                <Typography variant="h6" fontWeight={600}>
                  Staff Attendance - Today
                </Typography>
                <Chip
                  label="Admin"
                  size="small"
                  color="primary"
                  variant="outlined"
                />
              </Box>
              <Divider sx={{ mb: 2 }} />
              {loading ? (
                <Skeleton variant="rounded" height={120} />
              ) : (
                <Grid container spacing={3}>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="body1" fontWeight={600} gutterBottom>
                      Teachers
                    </Typography>
                    <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1 }}>
                      <Typography variant="h4" fontWeight={700} color={teachersPresent >= teachersMarked && teachersMarked > 0 ? 'success.main' : 'inherit'}>
                        {teachersPresent}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        present out of {teachersMarked || 0} marked / {totalTeachers} total
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={teachersMarked > 0 ? (teachersPresent / teachersMarked) * 100 : 0}
                      sx={{ mt: 1, height: 8, borderRadius: 1 }}
                    />
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                      {teachersMarked - teachersPresent} absent
                    </Typography>
                  </Grid>
                  <Grid size={{ xs: 12, md: 6 }}>
                    <Typography variant="body1" fontWeight={600} gutterBottom>
                      Employees
                    </Typography>
                    <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1 }}>
                      <Typography variant="h4" fontWeight={700} color={employeesPresent >= employeesMarked && employeesMarked > 0 ? 'success.main' : 'inherit'}>
                        {employeesPresent}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        present out of {employeesMarked || 0} marked / {totalEmployees} total
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant="determinate"
                      value={employeesMarked > 0 ? (employeesPresent / employeesMarked) * 100 : 0}
                      sx={{ mt: 1, height: 8, borderRadius: 1 }}
                    />
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                      {employeesMarked - employeesPresent} absent
                    </Typography>
                  </Grid>
                </Grid>
              )}
            </Paper>
          </Grid>
        </Grid>
      )}

      {isAdmin && (
        <Grid container spacing={3} sx={{ mt: 0.5 }}>
          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 3 }}>
              <Box
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  gap: 2,
                  mb: 2,
                  flexWrap: 'wrap',
                }}
              >
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <QuizIcon color="primary" />
                  <Typography variant="h6" fontWeight={600}>
                    Exam Results (Pass / Fail)
                  </Typography>
                  {filteredExamResults.length > 0 && (
                    <Chip
                      label={`${filteredExamResults.reduce(
                        (sum, r) => sum + Number(r.totalCount || 0),
                        0
                      )} students`}
                      size="small"
                      color="success"
                      variant="outlined"
                    />
                  )}
                </Box>
                {examOptions.length > 0 && (
                  <FormControl size="small" sx={{ minWidth: 220 }}>
                    <InputLabel id="exam-result-select-label">Exam</InputLabel>
                    <Select
                      labelId="exam-result-select-label"
                      id="exam-result-select"
                      value={activeExamId}
                      label="Exam"
                      onChange={(e) => setSelectedExamId(e.target.value)}
                    >
                      {examOptions.map((exam) => (
                        <MenuItem key={exam.id} value={exam.id}>
                          {exam.name}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                )}
              </Box>
              <Divider sx={{ mb: 2 }} />
              {loading ? (
                <Skeleton variant="rounded" height={300} />
              ) : filteredExamResults.length > 0 ? (
                <Box sx={{ height: 300 }}>
                  <Bar data={examChartData} options={CHART_OPTIONS} />
                </Box>
              ) : (
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{ py: 4, textAlign: 'center' }}
                >
                  No exam results recorded yet. Enter marks for an exam to see pass/fail
                  statistics.
                </Typography>
              )}
            </Paper>
          </Grid>
        </Grid>
      )}

      {canViewFees && (
        <Grid container spacing={3} sx={{ mt: 0.5 }}>
          <Grid size={{ xs: 12 }}>
            <Paper sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                <PaymentsIcon color="warning" />
                <Typography variant="h6" fontWeight={600}>
                  Students with Pending Fees
                </Typography>
                {pendingFeeStudents.length > 0 && (
                  <Chip
                    label={`${pendingFeeStudents.length} students`}
                    size="small"
                    color="warning"
                    variant="outlined"
                  />
                )}
              </Box>
              <Divider sx={{ mb: 1 }} />
              {loading ? (
                <Skeleton variant="rounded" height={120} />
              ) : pendingFeeStudents.length > 0 ? (
                <List disablePadding>
                  {pendingFeeStudents.map((item, index) => (
                    <React.Fragment key={item.studentId}>
                      <ListItem disablePadding sx={{ py: 1.5 }}>
                        <ListItemAvatar>
                          <Avatar
                            sx={{
                              bgcolor: item.isOverdue ? 'error.light' : 'warning.light',
                              color: 'white',
                              width: 40,
                              height: 40,
                              fontWeight: 600,
                              fontSize: '0.875rem',
                            }}
                          >
                            {item.studentName
                              .split(' ')
                              .map((n) => n[0])
                              .slice(0, 2)
                              .join('')}
                          </Avatar>
                        </ListItemAvatar>
                        <ListItemText
                          primary={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <Typography variant="body1" fontWeight={500}>
                                {item.studentName}
                              </Typography>
                              {item.isOverdue && (
                                <Chip label="Overdue" size="small" color="error" variant="outlined" />
                              )}
                            </Box>
                          }
                          secondary={`${item.className || '-'} · ${item.admissionNumber || ''}`}
                        />
                        <Typography variant="body1" fontWeight={700} color={item.isOverdue ? 'error.main' : 'warning.main'}>
                          ${Number(item.pendingAmount || 0).toFixed(2)}
                        </Typography>
                      </ListItem>
                      {index < pendingFeeStudents.length - 1 && <Divider />}
                    </React.Fragment>
                  ))}
                </List>
              ) : (
                <Typography variant="body2" color="text.secondary" sx={{ py: 2, textAlign: 'center' }}>
                  No students with pending fees
                </Typography>
              )}
            </Paper>
          </Grid>
        </Grid>
      )}
    </Box>
  );
}
