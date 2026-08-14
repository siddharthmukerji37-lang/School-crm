import React, { useEffect, useState } from 'react';
import { Box, Paper, Typography, Avatar, Chip, Divider, CircularProgress, Alert } from '@mui/material';
import Grid from '@mui/material/Grid2';
import PersonIcon from '@mui/icons-material/Person';
import SchoolIcon from '@mui/icons-material/School';
import BadgeIcon from '@mui/icons-material/Badge';
import WorkIcon from '@mui/icons-material/Work';
import authService from '../../services/authService';

function DetailRow({ label, value }) {
  return (
    <Box sx={{ display: 'flex', py: 1, flexWrap: 'wrap', gap: 1 }}>
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 180, fontWeight: 500 }}>
        {label}
      </Typography>
      <Typography variant="body2" sx={{ fontWeight: 500 }}>
        {value || '-'}
      </Typography>
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

function formatCurrency(value) {
  if (value === null || value === undefined) return '';
  return `$${Number(value).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
}

function SectionCard({ icon, title, children }) {
  return (
    <Paper sx={{ p: 3, mb: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        {icon}
        <Typography variant="h6" fontWeight={600}>
          {title}
        </Typography>
      </Box>
      <Divider sx={{ mb: 1 }} />
      {children}
    </Paper>
  );
}

export default function ProfilePage() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let active = true;
    authService
      .getMyProfile()
      .then((res) => {
        if (active) setProfile(res.data.data || null);
      })
      .catch(() => {
        if (active) setError('Failed to load your profile.');
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, []);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box>
        <Alert severity="error">{error}</Alert>
      </Box>
    );
  }

  const user = profile?.user || {};
  const student = profile?.student;
  const teacher = profile?.teacher;
  const employee = profile?.employee;

  const fullName = `${user.firstName || ''} ${user.lastName || ''}`.trim() || 'User';
  const role = user.roles?.[0] || '';

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} sx={{ mb: 3 }}>
        My Profile
      </Typography>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, flexWrap: 'wrap' }}>
          <Avatar
            src={user.profilePictureUrl || undefined}
            sx={{
              width: 88,
              height: 88,
              bgcolor: 'primary.main',
              fontSize: '2rem',
              fontWeight: 700,
            }}
          >
            {user.firstName?.charAt(0)}
            {user.lastName?.charAt(0)}
          </Avatar>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h5" fontWeight={600}>
              {fullName}
            </Typography>
            <Typography variant="body1" color="text.secondary">
              {user.email}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 1.5, flexWrap: 'wrap' }}>
              {(user.roles || []).map((r) => (
                <Chip key={r} label={r} color="primary" size="small" variant="outlined" />
              ))}
              {teacher && <Chip label="Staff" color="secondary" size="small" />}
              {employee && <Chip label="Employee" color="secondary" size="small" />}
              {student && (
                <Chip label={`${student.className || ''} • Sec ${student.sectionName || ''}`} color="success" size="small" />
              )}
            </Box>
          </Box>
        </Box>
      </Paper>

      <SectionCard icon={<PersonIcon color="primary" />} title="Personal Information">
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="First Name" value={user.firstName} />
            <DetailRow label="Last Name" value={user.lastName} />
            <DetailRow label="Email" value={user.email} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Phone" value={user.phone} />
            <DetailRow label="Gender" value={user.gender} />
            <DetailRow label="Date of Birth" value={formatDate(user.dateOfBirth)} />
          </Grid>
        </Grid>
      </SectionCard>

      {student && (
        <SectionCard icon={<SchoolIcon color="primary" />} title="Student Details">
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <DetailRow label="Admission Number" value={student.admissionNumber} />
              <DetailRow label="Roll Number" value={student.rollNumber} />
              <DetailRow label="Class" value={student.className} />
              <DetailRow label="Section" value={student.sectionName} />
              <DetailRow label="Admission Date" value={formatDate(student.admissionDate)} />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <DetailRow label="Parent Name" value={student.parentName} />
              <DetailRow label="Parent Phone" value={student.parentPhone} />
              <DetailRow label="Parent Email" value={student.parentEmail} />
              <DetailRow label="Blood Group" value={student.bloodGroup} />
              <DetailRow label="Address" value={student.address} />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <DetailRow label="Transport Required" value={student.transportRequired ? 'Yes' : 'No'} />
              <DetailRow label="Hostel Required" value={student.hostelRequired ? 'Yes' : 'No'} />
              <DetailRow label="Status" value={student.status} />
              <DetailRow label="Notes" value={student.notes} />
            </Grid>
          </Grid>
        </SectionCard>
      )}

      {teacher && (
        <SectionCard icon={<BadgeIcon color="primary" />} title="Teacher Details">
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <DetailRow label="Employee ID" value={teacher.employeeId} />
              <DetailRow label="Department" value={teacher.departmentName} />
              <DetailRow label="Designation" value={teacher.designation} />
              <DetailRow label="Qualification" value={teacher.qualification} />
              <DetailRow label="Specialization" value={teacher.specialization} />
              <DetailRow label="Experience" value={teacher.experience ? `${teacher.experience} years` : ''} />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <DetailRow label="Joining Date" value={formatDate(teacher.joiningDate)} />
              <DetailRow label="Salary" value={formatCurrency(teacher.salary)} />
              <DetailRow label="Blood Group" value={teacher.bloodGroup} />
              <DetailRow label="Address" value={teacher.address} />
              <DetailRow label="Status" value={teacher.status} />
            </Grid>
          </Grid>
        </SectionCard>
      )}

      {employee && (
        <SectionCard icon={<WorkIcon color="primary" />} title="Employee Details">
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <DetailRow label="Employee Code" value={employee.employeeCode} />
              <DetailRow label="Department" value={employee.departmentName} />
              <DetailRow label="Designation" value={employee.designation} />
              <DetailRow label="Employee Type" value={employee.employeeType} />
              <DetailRow label="Joining Date" value={formatDate(employee.joiningDate)} />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <DetailRow label="Salary" value={formatCurrency(employee.salary)} />
              <DetailRow label="Blood Group" value={employee.bloodGroup} />
              <DetailRow label="Address" value={employee.address} />
              <DetailRow label="Status" value={employee.status} />
            </Grid>
          </Grid>
        </SectionCard>
      )}
    </Box>
  );
}
