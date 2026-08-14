import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Typography, Card, CardContent, Chip, Stack, Avatar, CircularProgress } from '@mui/material';
import Grid from '@mui/material/Grid2';
import AddIcon from '@mui/icons-material/Add';
import SettingsIcon from '@mui/icons-material/Settings';
import CampaignIcon from '@mui/icons-material/Campaign';
import AnnouncementIcon from '@mui/icons-material/Announcement';
import { fetchPublishedNotices } from '../../store/slices/noticeSlice';
import PageHeader from '../../components/common/PageHeader';

const priorityColors = {
  High: 'error',
  Medium: 'warning',
  Low: 'info',
};

const typeIcons = {
  Announcement: <AnnouncementIcon />,
  Circular: <CampaignIcon />,
};

export default function NoticeBoardPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { user } = useSelector((state) => state.auth);
  const { publishedNotices, loading } = useSelector((state) => state.notices);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin');

  useEffect(() => {
    dispatch(fetchPublishedNotices());
  }, [dispatch]);

  return (
    <Box>
      <PageHeader
        title="Notice Board"
        subtitle="Announcements and circulars"
        actions={
          <Stack direction="row" spacing={1}>
            {isAdmin && (
              <Button
                variant="outlined"
                startIcon={<SettingsIcon />}
                onClick={() => navigate('/notices/manage')}
              >
                Manage
              </Button>
            )}
          </Stack>
        }
      />

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      ) : publishedNotices.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <CampaignIcon sx={{ fontSize: 64, color: 'text.disabled', mb: 2 }} />
          <Typography variant="h6" color="text.secondary">
            No notices on the board
          </Typography>
          <Typography variant="body2" color="text.disabled">
            Announcements and circulars will appear here.
          </Typography>
          {isAdmin && (
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => navigate('/notices/manage')}
              sx={{ mt: 2 }}
            >
              Create Notice
            </Button>
          )}
        </Box>
      ) : (
        <Grid container spacing={3}>
          {publishedNotices.map((notice) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={notice.id}>
              <Card
                sx={{
                  height: '100%',
                  display: 'flex',
                  flexDirection: 'column',
                  borderLeft: 4,
                  borderColor:
                    notice.priority === 'High'
                      ? 'error.main'
                      : notice.priority === 'Medium'
                      ? 'warning.main'
                      : 'info.main',
                }}
              >
                <CardContent sx={{ flex: 1 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 1.5 }}>
                    <Avatar
                      sx={{
                        bgcolor: notice.type === 'Announcement' ? 'primary.main' : 'secondary.main',
                        color: '#fff',
                        width: 36,
                        height: 36,
                        mr: 1.5,
                      }}
                    >
                      {typeIcons[notice.type] || <CampaignIcon />}
                    </Avatar>
                    <Stack direction="row" spacing={1} sx={{ flex: 1 }}>
                      <Chip
                        label={notice.type || 'Announcement'}
                        size="small"
                        variant="outlined"
                        sx={{ textTransform: 'capitalize' }}
                      />
                      <Chip
                        label={notice.priority || 'Medium'}
                        size="small"
                        color={priorityColors[notice.priority] || 'default'}
                      />
                    </Stack>
                  </Box>
                  <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                    {notice.title}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }}>
                    {notice.content}
                  </Typography>
                  <Stack direction="row" justifyContent="space-between" alignItems="center">
                    <Typography variant="caption" color="text.disabled">
                      {notice.publishDate
                        ? new Date(notice.publishDate).toLocaleDateString()
                        : notice.createdAt
                        ? new Date(notice.createdAt).toLocaleDateString()
                        : ''}
                    </Typography>
                    {notice.createdByName && (
                      <Typography variant="caption" color="text.disabled">
                        by {notice.createdByName}
                      </Typography>
                    )}
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Box>
  );
}
