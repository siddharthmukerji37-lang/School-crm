import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Paper, Typography, Avatar, Chip, Button, Divider, CircularProgress } from '@mui/material';
import Grid from '@mui/material/Grid2';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { fetchEmployeeById, clearSelectedEmployee } from '../../store/slices/employeeSlice';

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

export default function EmployeeDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedEmployee, loading } = useSelector((state) => state.employees);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin');

  useEffect(() => {
    dispatch(fetchEmployeeById(id));
    return () => {
      dispatch(clearSelectedEmployee());
    };
  }, [dispatch, id]);

  if (loading || !selectedEmployee) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const employee = selectedEmployee;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/employees')}
          variant="outlined"
        >
          Back
        </Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>
            Employee Profile
          </Typography>
        </Box>
        {isAdmin && (
          <Button
            variant="contained"
            startIcon={<EditIcon />}
            onClick={() => navigate(`/employees/${id}/edit`)}
          >
            Edit
          </Button>
        )}
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, flexWrap: 'wrap' }}>
          <Avatar
            src={employee.profilePictureUrl || undefined}
            sx={{
              width: 80,
              height: 80,
              bgcolor: 'primary.main',
              fontSize: '1.75rem',
              fontWeight: 700,
            }}
          >
            {employee.firstName?.charAt(0)}
            {employee.lastName?.charAt(0)}
          </Avatar>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h5" fontWeight={600}>
              {employee.firstName} {employee.lastName}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {employee.employeeCode}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
              <Chip label={employee.departmentName || 'No Department'} color="primary" size="small" />
              <Chip
                label={employee.status || 'Active'}
                color={employee.status === 'Inactive' ? 'default' : 'success'}
                size="small"
                variant="outlined"
              />
            </Box>
          </Box>
        </Box>
      </Paper>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
          Contact Information
        </Typography>
        <Divider sx={{ mb: 1 }} />
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="First Name" value={employee.firstName} />
            <DetailRow label="Last Name" value={employee.lastName} />
            <DetailRow label="Email" value={employee.email} />
            <DetailRow label="Phone" value={employee.phone} />
            <DetailRow label="Address" value={employee.address} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Gender" value={employee.gender} />
            <DetailRow label="Date of Birth" value={employee.dateOfBirth} />
            <DetailRow label="Date of Joining" value={employee.joiningDate} />
            <DetailRow label="Employee Type" value={employee.employeeType} />
            <DetailRow label="Blood Group" value={employee.bloodGroup} />
          </Grid>
        </Grid>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
          Employment Details
        </Typography>
        <Divider sx={{ mb: 1 }} />
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Employee Code" value={employee.employeeCode} />
            <DetailRow label="Department" value={employee.departmentName} />
            <DetailRow label="Designation" value={employee.designation} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Emergency Contact" value={employee.emergencyContactName} />
            <DetailRow label="Emergency Phone" value={employee.emergencyContactPhone} />
            <DetailRow label="Status" value={employee.status} />
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
}
