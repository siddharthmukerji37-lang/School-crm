import React, { useState, useEffect } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import * as signalR from '@microsoft/signalr';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Avatar,
  Menu,
  MenuItem,
  Box,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Badge,
  Tooltip,
  Divider,
  useMediaQuery,
} from '@mui/material';
import { useTheme } from '@mui/material/styles';
import MenuIcon from '@mui/icons-material/Menu';
import DashboardIcon from '@mui/icons-material/Dashboard';
import PeopleIcon from '@mui/icons-material/People';
import SchoolIcon from '@mui/icons-material/School';
import FamilyRestroomIcon from '@mui/icons-material/FamilyRestroom';
import BadgeIcon from '@mui/icons-material/Badge';
import EventAvailableIcon from '@mui/icons-material/EventAvailable';
import QuizIcon from '@mui/icons-material/Quiz';
import AssignmentIcon from '@mui/icons-material/Assignment';
import MenuBookIcon from '@mui/icons-material/MenuBook';
import DirectionsBusIcon from '@mui/icons-material/DirectionsBus';
import HotelIcon from '@mui/icons-material/Hotel';
import PaymentsIcon from '@mui/icons-material/Payments';
import InventoryIcon from '@mui/icons-material/Inventory';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import NotificationsIcon from '@mui/icons-material/Notifications';
import AssessmentIcon from '@mui/icons-material/Assessment';
import SettingsIcon from '@mui/icons-material/Settings';
import CampaignIcon from '@mui/icons-material/Campaign';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import LogoutIcon from '@mui/icons-material/Logout';
import PersonIcon from '@mui/icons-material/Person';
import ClassIcon from '@mui/icons-material/Class';
import ChildCareIcon from '@mui/icons-material/ChildCare';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';
import ChatIcon from '@mui/icons-material/Chat';
import PolicyIcon from '@mui/icons-material/Policy';
import MoneyOffIcon from '@mui/icons-material/MoneyOff';
import { toggleSidebar } from '../store/slices/uiSlice';
import { logout } from '../store/slices/authSlice';
import {
  fetchNotifications,
  fetchUnreadCount,
  markAsRead,
  markAllAsRead,
} from '../store/slices/notificationSlice';

const DRAWER_WIDTH = 260;
const COLLAPSED_WIDTH = 64;

const NAV_ITEMS_BY_ROLE = {
  SuperAdmin: [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Students', icon: <SchoolIcon />, path: '/students' },
    { label: 'Teachers', icon: <PeopleIcon />, path: '/teachers' },
    { label: 'Parents', icon: <FamilyRestroomIcon />, path: '/parents' },
    { label: 'Employees', icon: <BadgeIcon />, path: '/employees' },
    { label: 'Attendance', icon: <EventAvailableIcon />, path: '/attendance' },
    { label: 'Staff Attendance', icon: <EventAvailableIcon />, path: '/attendance/staff' },
    { label: 'Attendance Policy', icon: <PolicyIcon />, path: '/attendance/policy' },
    { label: 'Salary Deductions', icon: <MoneyOffIcon />, path: '/attendance/salary-deductions' },
    { label: 'Late Staff', icon: <PeopleIcon />, path: '/attendance/late-staff' },
    { label: 'Timetable', icon: <CalendarMonthIcon />, path: '/timetable' },
    { label: 'Exams', icon: <QuizIcon />, path: '/exams' },
    { label: 'Homework', icon: <AssignmentIcon />, path: '/homework' },
    { label: 'Library', icon: <MenuBookIcon />, path: '/library' },
    { label: 'Transport', icon: <DirectionsBusIcon />, path: '/transport' },
    { label: 'Hostel', icon: <HotelIcon />, path: '/hostel' },
    { label: 'Fees', icon: <PaymentsIcon />, path: '/fees' },
    { label: 'Inventory', icon: <InventoryIcon />, path: '/inventory' },
    { label: 'Accounts', icon: <AccountBalanceIcon />, path: '/accounts' },
    { label: 'Chat', icon: <ChatIcon />, path: '/chat' },
    { label: 'Notifications', icon: <NotificationsIcon />, path: '/notifications' },
    { label: 'Reports', icon: <AssessmentIcon />, path: '/reports' },
    { label: 'Settings', icon: <SettingsIcon />, path: '/settings' },
    { label: 'Notice Board', icon: <CampaignIcon />, path: '/notice-board' },
    { label: 'Notice Management', icon: <CampaignIcon />, path: '/notices/manage' },
  ],
  Admin: [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Students', icon: <SchoolIcon />, path: '/students' },
    { label: 'Parents', icon: <FamilyRestroomIcon />, path: '/parents' },
    { label: 'Employees', icon: <BadgeIcon />, path: '/employees' },
    { label: 'Attendance', icon: <EventAvailableIcon />, path: '/attendance' },
    { label: 'Staff Attendance', icon: <EventAvailableIcon />, path: '/attendance/staff' },
    { label: 'Attendance Policy', icon: <PolicyIcon />, path: '/attendance/policy' },
    { label: 'Salary Deductions', icon: <MoneyOffIcon />, path: '/attendance/salary-deductions' },
    { label: 'Late Staff', icon: <PeopleIcon />, path: '/attendance/late-staff' },
    { label: 'Timetable', icon: <CalendarMonthIcon />, path: '/timetable' },
    { label: 'Exams', icon: <QuizIcon />, path: '/exams' },
    { label: 'Homework', icon: <AssignmentIcon />, path: '/homework' },
    { label: 'Library', icon: <MenuBookIcon />, path: '/library' },
    { label: 'Transport', icon: <DirectionsBusIcon />, path: '/transport' },
    { label: 'Hostel', icon: <HotelIcon />, path: '/hostel' },
    { label: 'Fees', icon: <PaymentsIcon />, path: '/fees' },
    { label: 'Inventory', icon: <InventoryIcon />, path: '/inventory' },
    { label: 'Accounts', icon: <AccountBalanceIcon />, path: '/accounts' },
    { label: 'Chat', icon: <ChatIcon />, path: '/chat' },
    { label: 'Notifications', icon: <NotificationsIcon />, path: '/notifications' },
    { label: 'Reports', icon: <AssessmentIcon />, path: '/reports' },
    { label: 'Settings', icon: <SettingsIcon />, path: '/settings' },
    { label: 'Notice Board', icon: <CampaignIcon />, path: '/notice-board' },
    { label: 'Notice Management', icon: <CampaignIcon />, path: '/notices/manage' },
  ],
  Teacher: [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Students', icon: <SchoolIcon />, path: '/students' },
    { label: 'Teachers', icon: <PeopleIcon />, path: '/teachers' },
    { label: 'My Attendance', icon: <EventAvailableIcon />, path: '/my-attendance' },
    { label: 'Mark Attendance', icon: <EventAvailableIcon />, path: '/attendance/mark' },
    { label: 'Timetable', icon: <CalendarMonthIcon />, path: '/timetable' },
    { label: 'Parents', icon: <FamilyRestroomIcon />, path: '/parents' },
    { label: 'Exams', icon: <QuizIcon />, path: '/exams' },
    { label: 'Homework', icon: <AssignmentIcon />, path: '/homework' },
    { label: 'Transport', icon: <DirectionsBusIcon />, path: '/transport' },
    { label: 'Library', icon: <MenuBookIcon />, path: '/library' },
    { label: 'My Books', icon: <MenuBookIcon />, path: '/library/my-issues' },
    { label: 'Chat', icon: <ChatIcon />, path: '/chat' },
    { label: 'Notifications', icon: <NotificationsIcon />, path: '/notifications' },
    { label: 'Notice Board', icon: <CampaignIcon />, path: '/notice-board' },
  ],
  Student: [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Students', icon: <SchoolIcon />, path: '/students' },
    { label: 'Exams', icon: <QuizIcon />, path: '/exams' },
    { label: 'Homework', icon: <AssignmentIcon />, path: '/homework' },
    { label: 'Timetable', icon: <CalendarMonthIcon />, path: '/timetable' },
    { label: 'Transport', icon: <DirectionsBusIcon />, path: '/transport' },
    { label: 'Hostel', icon: <HotelIcon />, path: '/hostel' },
    { label: 'Library', icon: <MenuBookIcon />, path: '/library' },
    { label: 'My Books', icon: <MenuBookIcon />, path: '/library/my-issues' },
    { label: 'My Fees', icon: <PaymentsIcon />, path: '/fees/my-receipts' },
    { label: 'Chat', icon: <ChatIcon />, path: '/chat' },
    { label: 'Notifications', icon: <NotificationsIcon />, path: '/notifications' },
    { label: 'Notice Board', icon: <CampaignIcon />, path: '/notice-board' },
  ],
  Parent: [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'My Children', icon: <ChildCareIcon />, path: '/students' },
    { label: 'Attendance', icon: <EventAvailableIcon />, path: '/attendance' },
    { label: 'Exams', icon: <QuizIcon />, path: '/exams' },
    { label: 'Fees', icon: <PaymentsIcon />, path: '/fees' },
    { label: 'Library', icon: <MenuBookIcon />, path: '/library' },
    { label: 'Chat', icon: <ChatIcon />, path: '/chat' },
    { label: 'Notifications', icon: <NotificationsIcon />, path: '/notifications' },
    { label: 'Notice Board', icon: <CampaignIcon />, path: '/notice-board' },
  ],
  Librarian: [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Library', icon: <MenuBookIcon />, path: '/library' },
    { label: 'Chat', icon: <ChatIcon />, path: '/chat' },
    { label: 'Notifications', icon: <NotificationsIcon />, path: '/notifications' },
    { label: 'Notice Board', icon: <CampaignIcon />, path: '/notice-board' },
  ],
  Accountant: [
    { label: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { label: 'Fees', icon: <PaymentsIcon />, path: '/fees' },
    { label: 'Accounts', icon: <AccountBalanceIcon />, path: '/accounts' },
    { label: 'Inventory', icon: <InventoryIcon />, path: '/inventory' },
    { label: 'Chat', icon: <ChatIcon />, path: '/chat' },
    { label: 'Notifications', icon: <NotificationsIcon />, path: '/notifications' },
    { label: 'Notice Board', icon: <CampaignIcon />, path: '/notice-board' },
  ],
};

function Breadcrumbs({ pathname }) {
  const segments = pathname.split('/').filter(Boolean);

  if (segments.length === 0 || segments[0] === 'dashboard') {
    return (
      <Typography variant="body2" color="text.secondary">
        Dashboard
      </Typography>
    );
  }

  const crumbs = segments.map((segment, index) => {
    const path = '/' + segments.slice(0, index + 1).join('/');
    const label = segment
      .split('-')
      .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
    return { label, path };
  });

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
      {crumbs.map((crumb, index) => (
        <React.Fragment key={crumb.path}>
          {index > 0 && (
            <Typography variant="body2" color="text.secondary" sx={{ mx: 0.5 }}>
              /
            </Typography>
          )}
          <Typography
            variant="body2"
            color={index === crumbs.length - 1 ? 'primary' : 'text.secondary'}
            fontWeight={index === crumbs.length - 1 ? 600 : 400}
          >
            {crumb.label}
          </Typography>
        </React.Fragment>
      ))}
    </Box>
  );
}

export default function AdminLayout() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const { sidebarOpen } = useSelector((state) => state.ui);
  const { user, unreadCount } = useSelector((state) => ({
    user: state.auth.user,
    unreadCount: state.notifications.unreadCount,
  }));
  const chatUnread = useSelector((state) => state.chat.unreadTotal);
  const notifications = useSelector((state) => state.notifications.notifications) || [];

  const userRole = user?.roles?.[0] || user?.role || 'Admin';
  const isAdmin = ['SuperAdmin', 'Admin'].includes(userRole);
  const navItems = NAV_ITEMS_BY_ROLE[userRole] || NAV_ITEMS_BY_ROLE.Admin;

  const [anchorEl, setAnchorEl] = useState(null);
  const [notificationAnchorEl, setNotificationAnchorEl] = useState(null);

  useEffect(() => {
    dispatch(fetchUnreadCount());
    const timer = setInterval(() => {
      dispatch(fetchUnreadCount());
    }, 30000);
    return () => clearInterval(timer);
  }, [dispatch]);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token || !user) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveNotification', () => {
      dispatch(fetchUnreadCount());
      dispatch(fetchNotifications({ pageSize: 8 }));
    });

    connection.start().catch(() => {});

    return () => {
      connection.stop().catch(() => {});
    };
  }, [dispatch, user]);

  const handleNotificationOpen = (event) => {
    setNotificationAnchorEl(event.currentTarget);
    dispatch(fetchNotifications({ pageSize: 8 }));
  };

  const handleNotificationClose = () => {
    setNotificationAnchorEl(null);
  };

  const handleNotificationClick = async (notification) => {
    if (!notification.isRead) {
      await dispatch(markAsRead(notification.id));
    }
    handleNotificationClose();
    if (userRole === 'Student' || userRole === 'Parent') {
      navigate('/notifications');
    } else if (notification.link) {
      navigate(notification.link);
    } else {
      navigate('/notifications');
    }
  };

  const handleMarkAllRead = async () => {
    await dispatch(markAllAsRead());
    dispatch(fetchUnreadCount());
  };

  const handleDrawerToggle = () => {
    dispatch(toggleSidebar());
  };

  const handleProfileMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleProfileMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    handleProfileMenuClose();
    dispatch(logout());
    navigate('/login');
  };

  const handleNavigation = (path) => {
    navigate(path);
    if (isMobile) {
      dispatch(toggleSidebar());
    }
  };

  const drawerWidth = sidebarOpen ? DRAWER_WIDTH : COLLAPSED_WIDTH;

  const drawerContent = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: sidebarOpen ? 'space-between' : 'center',
          px: sidebarOpen ? 2 : 1,
          py: 2,
          minHeight: 64,
        }}
      >
        {sidebarOpen && (
          <Typography variant="h6" noWrap sx={{ fontWeight: 700, color: 'primary.main' }}>
            School CRM
          </Typography>
        )}
        <IconButton onClick={handleDrawerToggle} size="small">
          {sidebarOpen ? <ChevronLeftIcon /> : <MenuIcon />}
        </IconButton>
      </Box>

      <Divider />

      <List sx={{ flex: 1, px: 1, py: 1 }}>
        {navItems.map((item) => {
          const isActive = location.pathname === item.path ||
            location.pathname.startsWith(item.path + '/');

          return (
            <ListItem key={item.path} disablePadding sx={{ mb: 0.5 }}>
              <Tooltip title={!sidebarOpen ? item.label : ''} placement="right" arrow>
                <ListItemButton
                  onClick={() => handleNavigation(item.path)}
                  sx={{
                    borderRadius: 2,
                    minHeight: 44,
                    justifyContent: sidebarOpen ? 'initial' : 'center',
                    px: sidebarOpen ? 2 : 1.5,
                    backgroundColor: isActive
                      ? 'primary.main'
                      : 'transparent',
                    color: isActive ? 'white' : 'text.secondary',
                    '&:hover': {
                      backgroundColor: isActive
                        ? 'primary.dark'
                        : 'action.hover',
                    },
                    '& .MuiListItemIcon-root': {
                      color: isActive ? 'white' : 'text.secondary',
                      minWidth: 0,
                      mr: sidebarOpen ? 2 : 0,
                      justifyContent: 'center',
                    },
                  }}
                >
                  <ListItemIcon>{item.icon}</ListItemIcon>
                  {sidebarOpen && <ListItemText primary={item.label} />}
                </ListItemButton>
              </Tooltip>
            </ListItem>
          );
        })}
      </List>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      {isMobile ? (
        <Drawer
          variant="temporary"
          open={sidebarOpen}
          onClose={handleDrawerToggle}
          ModalProps={{ keepMounted: true }}
          sx={{
            '& .MuiDrawer-paper': {
              width: DRAWER_WIDTH,
              boxSizing: 'border-box',
            },
          }}
        >
          {drawerContent}
        </Drawer>
      ) : (
        <Drawer
          variant="persistent"
          open={sidebarOpen}
          sx={{
            width: drawerWidth,
            flexShrink: 0,
            transition: 'width 0.2s ease-in-out',
            '& .MuiDrawer-paper': {
              width: drawerWidth,
              boxSizing: 'border-box',
              transition: 'width 0.2s ease-in-out',
              overflowX: 'hidden',
            },
          }}
        >
          {drawerContent}
        </Drawer>
      )}

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          display: 'flex',
          flexDirection: 'column',
          minWidth: 0,
          transition: 'margin 0.2s ease-in-out',
          ml: isMobile ? 0 : `${sidebarOpen ? 0 : -COLLAPSED_WIDTH + DRAWER_WIDTH}px`,
        }}
      >
        <AppBar
          position="sticky"
          color="default"
          sx={{
            backgroundColor: 'background.paper',
            borderBottom: '1px solid',
            borderColor: 'divider',
          }}
        >
          <Toolbar sx={{ justifyContent: 'space-between' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              {isMobile && (
                <IconButton
                  color="inherit"
                  edge="start"
                  onClick={handleDrawerToggle}
                >
                  <MenuIcon />
                </IconButton>
              )}
              <Breadcrumbs pathname={location.pathname} />
            </Box>

            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <Tooltip title="Chat">
                <IconButton color="inherit" onClick={() => navigate('/chat')}>
                  <Badge badgeContent={chatUnread} color="error">
                    <ChatIcon />
                  </Badge>
                </IconButton>
              </Tooltip>

              <Tooltip title="Notifications">
                <IconButton
                  color="inherit"
                  onClick={handleNotificationOpen}
                >
                  <Badge badgeContent={unreadCount} color="error">
                    <NotificationsIcon />
                  </Badge>
                </IconButton>
              </Tooltip>

              <Menu
                anchorEl={notificationAnchorEl}
                open={Boolean(notificationAnchorEl)}
                onClose={handleNotificationClose}
                transformOrigin={{ horizontal: 'right', vertical: 'top' }}
                anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
                slotProps={{
                  paper: { sx: { width: 360, maxHeight: 420, borderRadius: 2 } },
                }}
              >
                <Box sx={{ px: 2, py: 1.5, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="subtitle1" fontWeight={600}>Notifications</Typography>
                  <Typography
                    component="span"
                    variant="caption"
                    sx={{ color: 'primary.main', cursor: 'pointer', fontWeight: 600 }}
                    onClick={handleMarkAllRead}
                  >
                    Mark all read
                  </Typography>
                </Box>
                <Divider />
                {notifications.length === 0 ? (
                  <Box sx={{ px: 2, py: 4, textAlign: 'center' }}>
                    <Typography variant="body2" color="text.secondary">No notifications yet</Typography>
                  </Box>
                ) : (
                  notifications.slice(0, 6).map((notification) => (
                    <MenuItem
                      key={notification.id}
                      onClick={() => handleNotificationClick(notification)}
                      sx={{ whiteSpace: 'normal', alignItems: 'flex-start' }}
                    >
                      <Box sx={{ width: '100%' }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 1 }}>
                          <Typography variant="body2" fontWeight={600} sx={{ flex: 1 }}>
                            {notification.title}
                          </Typography>
                          {!notification.isRead && (
                            <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: 'error.main', mt: 0.8, flexShrink: 0 }} />
                          )}
                        </Box>
                        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.25 }}>
                          {notification.message}
                        </Typography>
                        <Typography variant="caption" color="text.disabled">
                          {notification.createdAt ? new Date(notification.createdAt).toLocaleString() : ''}
                        </Typography>
                      </Box>
                    </MenuItem>
                  ))
                )}
                <Divider />
                <MenuItem onClick={() => { handleNotificationClose(); navigate('/notifications'); }}>
                  <Typography variant="body2" fontWeight={600} sx={{ color: 'primary.main', width: '100%', textAlign: 'center' }}>
                    View all notifications
                  </Typography>
                </MenuItem>
              </Menu>

              <Tooltip title="Account">
                <IconButton onClick={handleProfileMenuOpen} sx={{ ml: 1 }}>
                  <Avatar
                    src={user?.profilePictureUrl || undefined}
                    sx={{
                      width: 36,
                      height: 36,
                      bgcolor: 'primary.main',
                      fontSize: '0.875rem',
                      fontWeight: 600,
                    }}
                  >
                    {user?.firstName?.charAt(0) || 'U'}
                  </Avatar>
                </IconButton>
              </Tooltip>
            </Box>
          </Toolbar>
        </AppBar>

        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleProfileMenuClose}
          transformOrigin={{ horizontal: 'right', vertical: 'top' }}
          anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
        >
          <MenuItem
            onClick={() => {
              handleProfileMenuClose();
              handleNavigation('/profile');
            }}
          >
            <ListItemIcon>
              <PersonIcon fontSize="small" />
            </ListItemIcon>
            My Profile
          </MenuItem>
          {isAdmin && (
            <MenuItem
              onClick={() => {
                handleProfileMenuClose();
                handleNavigation('/settings');
              }}
            >
              <ListItemIcon>
                <SettingsIcon fontSize="small" />
              </ListItemIcon>
              Settings
            </MenuItem>
          )}
          <Divider />
          <MenuItem onClick={handleLogout}>
            <ListItemIcon>
              <LogoutIcon fontSize="small" />
            </ListItemIcon>
            Logout
          </MenuItem>
        </Menu>

        <Box
          sx={{
            flex: 1,
            p: { xs: 2, md: 3 },
            backgroundColor: 'background.default',
            overflow: 'auto',
          }}
        >
          <Outlet />
        </Box>
      </Box>
    </Box>
  );
}
