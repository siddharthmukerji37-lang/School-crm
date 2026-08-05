import React, { useEffect, useState } from 'react';
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
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import PersonIcon from '@mui/icons-material/Person';
import EventAvailableIcon from '@mui/icons-material/EventAvailable';
import QuizIcon from '@mui/icons-material/Quiz';
import PaymentsIcon from '@mui/icons-material/Payments';
import FolderOpenIcon from '@mui/icons-material/FolderOpen';
import { fetchStudentById, clearSelectedStudent } from '../../store/slices/studentSlice';

function TabPanel({ children, value, index, ...other }) {
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

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

export default function StudentDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedStudent, loading } = useSelector((state) => state.students);
  const [tabValue, setTabValue] = useState(0);

  useEffect(() => {
    dispatch(fetchStudentById(id));
    return () => {
      dispatch(clearSelectedStudent());
    };
  }, [dispatch, id]);

  if (loading || !selectedStudent) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const student = selectedStudent;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/students')}
          variant="outlined"
        >
          Back
        </Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>
            Student Profile
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<EditIcon />}
          onClick={() => navigate(`/students/${id}/edit`)}
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
            {student.firstName?.charAt(0)}
            {student.lastName?.charAt(0)}
          </Avatar>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h5" fontWeight={600}>
              {student.firstName} {student.lastName}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
              <Chip label={student.className || 'N/A'} color="primary" size="small" />
              <Chip label={student.sectionName || 'N/A'} color="secondary" size="small" />
              <Chip
                label={student.status || 'Active'}
                color={student.status === 'Inactive' ? 'default' : 'success'}
                size="small"
                variant="outlined"
              />
            </Box>
          </Box>
        </Box>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Tabs
          value={tabValue}
          onChange={(_, newValue) => setTabValue(newValue)}
          variant="scrollable"
          scrollButtons="auto"
          sx={{
            borderBottom: 1,
            borderColor: 'divider',
            mb: 0,
            '& .MuiTab-root': { minHeight: 48 },
          }}
        >
          <Tab icon={<PersonIcon />} iconPosition="start" label="Profile" />
          <Tab icon={<EventAvailableIcon />} iconPosition="start" label="Attendance" />
          <Tab icon={<QuizIcon />} iconPosition="start" label="Marks" />
          <Tab icon={<PaymentsIcon />} iconPosition="start" label="Fees" />
          <Tab icon={<FolderOpenIcon />} iconPosition="start" label="Documents" />
        </Tabs>

        <TabPanel value={tabValue} index={0}>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
                Personal Information
              </Typography>
              <Divider sx={{ mb: 1 }} />
              <DetailRow label="First Name" value={student.firstName} />
              <DetailRow label="Last Name" value={student.lastName} />
              <DetailRow label="Email" value={student.email} />
              <DetailRow label="Phone" value={student.phone} />
              <DetailRow label="Date of Birth" value={student.dateOfBirth ? new Date(student.dateOfBirth).toLocaleDateString() : ''} />
              <DetailRow label="Gender" value={student.gender} />
              <DetailRow label="Blood Group" value={student.bloodGroup} />
              <DetailRow label="Address" value={student.address} />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
                Academic Information
              </Typography>
              <Divider sx={{ mb: 1 }} />
              <DetailRow label="Admission Number" value={student.admissionNumber} />
              <DetailRow label="Admission Date" value={student.admissionDate ? new Date(student.admissionDate).toLocaleDateString() : ''} />
              <DetailRow label="Class" value={student.className} />
              <DetailRow label="Section" value={student.sectionName} />

              <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ mt: 2, color: 'primary.main' }}>
                Parent/Guardian Information
              </Typography>
              <Divider sx={{ mb: 1 }} />
              <DetailRow label="Parent Name" value={student.parentName} />
              <DetailRow label="Parent Phone" value={student.parentPhone} />
              <DetailRow label="Parent Email" value={student.parentEmail} />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
                Additional Information
              </Typography>
              <Divider sx={{ mb: 1 }} />
              <DetailRow label="Transport Required" value={student.transportRequired ? 'Yes' : 'No'} />
              <DetailRow label="Hostel Required" value={student.hostelRequired ? 'Yes' : 'No'} />
              <DetailRow label="Notes" value={student.notes} />
            </Grid>
          </Grid>
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
            Attendance records will be displayed here.
          </Typography>
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
            Exam marks will be displayed here.
          </Typography>
        </TabPanel>

        <TabPanel value={tabValue} index={3}>
          <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
            Fee records will be displayed here.
          </Typography>
        </TabPanel>

        <TabPanel value={tabValue} index={4}>
          <Typography variant="body1" color="text.secondary" sx={{ textAlign: 'center', py: 4 }}>
            Documents will be displayed here.
          </Typography>
        </TabPanel>
      </Paper>
    </Box>
  );
}
