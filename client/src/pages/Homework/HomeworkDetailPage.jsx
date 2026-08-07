import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Paper, Typography, Grid, Chip, Button, Divider, CircularProgress } from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { fetchHomeworkById, clearSelectedHomework } from '../../store/slices/homeworkSlice';

function DetailRow({ label, value }) {
  return (
    <Box sx={{ display: 'flex', py: 1 }}>
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 180, fontWeight: 500 }}>
        {label}
      </Typography>
      <Typography variant="body2">{value || '-'}</Typography>
    </Box>
  );
}

export default function HomeworkDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedHomework, loading } = useSelector((state) => state.homework);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin' || r === 'Teacher');

  useEffect(() => {
    dispatch(fetchHomeworkById(id));
    return () => {
      dispatch(clearSelectedHomework());
    };
  }, [dispatch, id]);

  if (loading || !selectedHomework) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const hw = selectedHomework;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/homework')} variant="outlined">
          Back
        </Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>
            Homework Details
          </Typography>
        </Box>
        {isAdmin && (
          <Button
            variant="contained"
            startIcon={<EditIcon />}
            onClick={() => navigate(`/homework/${id}/edit`)}
          >
            Edit
          </Button>
        )}
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h5" fontWeight={600}>
              {hw.title}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
              <Chip label={hw.subjectName || 'No Subject'} color="primary" size="small" />
              <Chip label={hw.className || 'No Class'} color="secondary" size="small" variant="outlined" />
              <Chip
                label={hw.isActive ? 'Active' : 'Inactive'}
                color={hw.isActive ? 'success' : 'default'}
                size="small"
                variant="outlined"
              />
            </Box>
          </Box>
        </Box>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
          Assignment Information
        </Typography>
        <Divider sx={{ mb: 1 }} />
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Description" value={hw.description} />
            <DetailRow label="Subject" value={hw.subjectName} />
            <DetailRow label="Class" value={hw.className} />
            <DetailRow label="Section" value={hw.sectionName} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Assigned By" value={hw.teacherName} />
            <DetailRow label="Assigned Date" value={hw.assignedDate} />
            <DetailRow label="Due Date" value={hw.dueDate} />
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
}
