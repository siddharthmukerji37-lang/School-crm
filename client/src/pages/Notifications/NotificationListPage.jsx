import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box,
  Button,
  Typography,
  List,
  ListItem,
  ListItemAvatar,
  ListItemText,
  Avatar,
  Badge,
  IconButton,
  Chip,
  CircularProgress,
  Divider,
} from '@mui/material';
import NotificationsIcon from '@mui/icons-material/Notifications';
import MarkEmailReadIcon from '@mui/icons-material/MarkEmailRead';
import InfoIcon from '@mui/icons-material/Info';
import WarningIcon from '@mui/icons-material/Warning';
import ErrorIcon from '@mui/icons-material/Error';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import { fetchNotifications, markAsRead, markAllAsRead } from '../../store/slices/notificationSlice';
import PageHeader from '../../components/common/PageHeader';
import toast from 'react-hot-toast';

const notificationIconMap = {
  info: <InfoIcon />,
  warning: <WarningIcon />,
  error: <ErrorIcon />,
  success: <CheckCircleIcon />,
};

const notificationColorMap = {
  info: 'primary.main',
  warning: 'warning.main',
  error: 'error.main',
  success: 'success.main',
};

function timeAgo(dateString) {
  if (!dateString) return '';
  const now = new Date();
  const date = new Date(dateString);
  const seconds = Math.floor((now - date) / 1000);
  if (seconds < 60) return 'just now';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 7) return `${days}d ago`;
  return date.toLocaleDateString();
}

export default function NotificationListPage() {
  const dispatch = useDispatch();
  const { notifications, loading, unreadCount } = useSelector((state) => state.notifications);
  const [markingAll, setMarkingAll] = useState(false);

  useEffect(() => {
    dispatch(fetchNotifications());
  }, [dispatch]);

  const handleMarkAllAsRead = async () => {
    setMarkingAll(true);
    const result = await dispatch(markAllAsRead());
    setMarkingAll(false);
    if (markAllAsRead.fulfilled.match(result)) {
      toast.success('All notifications marked as read');
    } else {
      toast.error(result.payload || 'Failed to mark all as read');
    }
  };

  const handleMarkAsRead = async (notification) => {
    if (notification.isRead) return;
    const result = await dispatch(markAsRead(notification.id));
    if (markAsRead.rejected.match(result)) {
      toast.error(result.payload || 'Failed to mark notification as read');
    }
  };

  return (
    <Box>
      <PageHeader
        title="Notifications"
        subtitle={`You have ${unreadCount || 0} unread notification(s)`}
        actions={
          <Button
            variant="outlined"
            startIcon={<MarkEmailReadIcon />}
            onClick={handleMarkAllAsRead}
            disabled={markingAll || unreadCount === 0}
          >
            {markingAll ? 'Marking...' : 'Mark All as Read'}
          </Button>
        }
      />

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      ) : notifications.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <NotificationsIcon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
          <Typography variant="h6" color="text.secondary">
            No notifications
          </Typography>
          <Typography variant="body2" color="text.disabled">
            You're all caught up!
          </Typography>
        </Box>
      ) : (
        <List sx={{ bgcolor: 'background.paper', borderRadius: 2, border: '1px solid', borderColor: 'divider' }}>
          {notifications.map((notification, index) => (
            <React.Fragment key={notification.id}>
              <ListItem
                alignItems="flex-start"
                onClick={() => handleMarkAsRead(notification)}
                sx={{
                  cursor: 'pointer',
                  bgcolor: notification.isRead ? 'transparent' : 'action.hover',
                  '&:hover': { bgcolor: 'action.selected' },
                  py: 2,
                }}
              >
                <ListItemAvatar>
                  <Badge
                    variant="dot"
                    color="primary"
                    invisible={notification.isRead}
                    sx={{ '& .MuiBadge-badge': { top: 8, right: 8 } }}
                  >
                    <Avatar
                      sx={{
                        bgcolor: notificationColorMap[notification.type] || 'grey.500',
                        color: '#fff',
                        width: 44,
                        height: 44,
                      }}
                    >
                      {notificationIconMap[notification.type] || <NotificationsIcon />}
                    </Avatar>
                  </Badge>
                </ListItemAvatar>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                      <Typography
                        variant="subtitle1"
                        sx={{ fontWeight: notification.isRead ? 400 : 700, flex: 1 }}
                      >
                        {notification.title || 'Notification'}
                      </Typography>
                      <Typography variant="caption" color="text.secondary" sx={{ flexShrink: 0 }}>
                        {timeAgo(notification.createdAt)}
                      </Typography>
                    </Box>
                  }
                  secondary={
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{
                        fontWeight: notification.isRead ? 400 : 500,
                        display: '-webkit-box',
                        WebkitLineClamp: 2,
                        WebkitBoxOrient: 'vertical',
                        overflow: 'hidden',
                      }}
                    >
                      {notification.message}
                    </Typography>
                  }
                />
                {notification.type && (
                  <Chip
                    label={notification.type}
                    size="small"
                    variant="outlined"
                    sx={{ alignSelf: 'flex-start', mt: 0.5, textTransform: 'capitalize' }}
                  />
                )}
              </ListItem>
              {index < notifications.length - 1 && <Divider variant="inset" component="li" />}
            </React.Fragment>
          ))}
        </List>
      )}
    </Box>
  );
}
