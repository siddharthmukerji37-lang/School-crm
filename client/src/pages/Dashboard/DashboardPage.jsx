import React, { useEffect } from 'react';
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
import { Line, Doughnut } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  ArcElement,
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
      title: "Today's Attendance",
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
    </Box>
  );
}
