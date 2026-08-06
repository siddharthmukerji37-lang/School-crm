import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box,
  Paper,
  Typography,
  Grid,
  Avatar,
  Chip,
  Button,
  Divider,
  CircularProgress,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { fetchTeacherById, clearSelectedTeacher } from '../../store/slices/teacherSlice';

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

function formatDate(value) {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  const day = String(date.getDate()).padStart(2, '0');
  const month = date.toLocaleDateString(undefined, { month: 'short' });
  const year = date.getFullYear();
  return `${day} ${month} ${year}`;
}

export default function TeacherDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedTeacher, loading } = useSelector((state) => state.teachers);

  useEffect(() => {
    dispatch(fetchTeacherById(id));
    return () => {
      dispatch(clearSelectedTeacher());
    };
  }, [dispatch, id]);

  if (loading || !selectedTeacher) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const teacher = selectedTeacher;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/teachers')}
          variant="outlined"
        >
          Back
        </Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>
            Teacher Profile
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<EditIcon />}
          onClick={() => navigate(`/teachers/${id}/edit`)}
        >
          Edit
        </Button>
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, flexWrap: 'wrap' }}>
          <Avatar
            sx={{
              width: 80,
              height: 80,
              bgcolor: 'primary.main',
              fontSize: '1.75rem',
              fontWeight: 700,
            }}
          >
            {teacher.firstName?.charAt(0)}
            {teacher.lastName?.charAt(0)}
          </Avatar>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h5" fontWeight={600}>
              {teacher.firstName} {teacher.lastName}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
              <Chip label={teacher.employeeId || 'N/A'} color="primary" size="small" />
              <Chip label={teacher.departmentName || 'N/A'} color="secondary" size="small" />
              <Chip
                label={teacher.status || 'Active'}
                color={teacher.status === 'Inactive' ? 'default' : 'success'}
                size="small"
                variant="outlined"
              />
            </Box>
          </Box>
        </Box>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
              Personal Information
            </Typography>
            <Divider sx={{ mb: 1 }} />
            <DetailRow label="Employee ID" value={teacher.employeeId} />
            <DetailRow label="First Name" value={teacher.firstName} />
            <DetailRow label="Last Name" value={teacher.lastName} />
            <DetailRow label="Email" value={teacher.email} />
            <DetailRow label="Phone" value={teacher.phone} />
            <DetailRow label="Date of Birth" value={formatDate(teacher.dateOfBirth)} />
            <DetailRow label="Gender" value={teacher.gender} />
            <DetailRow label="Blood Group" value={teacher.bloodGroup} />
            <DetailRow label="Address" value={teacher.address} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
              Employment Information
            </Typography>
            <Divider sx={{ mb: 1 }} />
            <DetailRow label="Department" value={teacher.departmentName} />
            <DetailRow label="Designation" value={teacher.designation} />
            <DetailRow label="Joining Date" value={formatDate(teacher.joiningDate)} />
            <DetailRow label="Qualification" value={teacher.qualification} />
            <DetailRow label="Specialization" value={teacher.specialization} />
            <DetailRow label="Experience" value={teacher.experience != null ? `${teacher.experience} years` : null} />
            <DetailRow label="Salary" value={teacher.salary != null ? `₹${teacher.salary}` : null} />
            <DetailRow label="Status" value={teacher.status} />
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
}
