import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Formik, Form } from 'formik';
import {
  Box,
  Grid,
  TextField,
  MenuItem,
  Button,
  Paper,
  Typography,
  CircularProgress,
  Divider,
  Stack,
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import * as Yup from 'yup';
import { createTeacher, updateTeacher, fetchTeacherById } from '../../store/slices/teacherSlice';
import toast from 'react-hot-toast';

const GENDER_OPTIONS = ['Male', 'Female', 'Other'];
const STATUS_OPTIONS = ['Active', 'OnLeave', 'Inactive'];
const DEPARTMENT_OPTIONS = [
  'Mathematics',
  'Science',
  'English',
  'History',
  'Geography',
  'Computer Science',
  'Arts',
  'Physical Education',
];

const teacherSchema = Yup.object({
  firstName: Yup.string().trim().required('First name is required'),
  lastName: Yup.string().trim().required('Last name is required'),
  email: Yup.string().email('Invalid email').required('Email is required'),
  phone: Yup.string().matches(/^[0-9+\-\s()]*$/, 'Invalid phone number'),
  employeeId: Yup.string().trim().required('Employee ID is required'),
  departmentName: Yup.string().required('Department is required'),
  gender: Yup.string().oneOf(['Male', 'Female', 'Other']).required('Gender is required'),
  dateOfJoining: Yup.date().nullable().required('Date of joining is required'),
  qualification: Yup.string().trim(),
  experience: Yup.number().transform((value, originalValue) =>
    originalValue === '' ? undefined : value
  ).min(0, 'Must be positive').max(50, 'Must be 50 or less'),
  address: Yup.string().trim(),
  password: Yup.string()
    .min(6, 'Password must be at least 6 characters'),
});

export default function TeacherFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedTeacher, loading } = useSelector((state) => state.teachers);
  const isEditMode = Boolean(id);

  const [initialValues, setInitialValues] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    employeeId: '',
    departmentName: '',
    gender: '',
    dateOfJoining: '',
    qualification: '',
    experience: '',
    address: '',
    password: '',
    status: 'Active',
  });

  useEffect(() => {
    if (isEditMode) {
      dispatch(fetchTeacherById(id));
    }
  }, [dispatch, id, isEditMode]);

  useEffect(() => {
    if (isEditMode && selectedTeacher) {
      setInitialValues({
        firstName: selectedTeacher.firstName || '',
        lastName: selectedTeacher.lastName || '',
        email: selectedTeacher.email || '',
        phone: selectedTeacher.phone || '',
        employeeId: selectedTeacher.employeeId || '',
        departmentName: selectedTeacher.departmentName || '',
        gender: selectedTeacher.gender || '',
        dateOfJoining: selectedTeacher.dateOfJoining
          ? new Date(selectedTeacher.dateOfJoining).toISOString().split('T')[0]
          : '',
        qualification: selectedTeacher.qualification || '',
        experience: selectedTeacher.experience ?? '',
        address: selectedTeacher.address || '',
        password: '',
        status: selectedTeacher.status || 'Active',
      });
    }
  }, [isEditMode, selectedTeacher]);

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      const payload = {
        ...values,
        experience: values.experience !== '' ? Number(values.experience) : null,
      };
      if (isEditMode) {
        delete payload.password;
        const result = await dispatch(updateTeacher({ id, data: payload }));
        if (updateTeacher.fulfilled.match(result)) {
          toast.success('Teacher updated successfully');
          navigate('/teachers');
        } else {
          toast.error(result.payload || 'Failed to update teacher');
        }
      } else {
        const result = await dispatch(createTeacher(payload));
        if (createTeacher.fulfilled.match(result)) {
          toast.success('Teacher created successfully');
          navigate('/teachers');
        } else {
          toast.error(result.payload || 'Failed to create teacher');
        }
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (isEditMode && loading && !selectedTeacher) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

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
        <Typography variant="h4" fontWeight={700}>
          {isEditMode ? 'Edit Teacher' : 'Add New Teacher'}
        </Typography>
      </Box>

      <Formik
        initialValues={initialValues}
        validationSchema={teacherSchema}
        onSubmit={handleSubmit}
        enableReinitialize
      >
        {({
          values,
          errors,
          touched,
          handleChange,
          handleBlur,
          isSubmitting,
        }) => (
          <Form>
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Personal Information
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="firstName"
                    label="First Name"
                    value={values.firstName}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.firstName && Boolean(errors.firstName)}
                    helperText={touched.firstName && errors.firstName}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="lastName"
                    label="Last Name"
                    value={values.lastName}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.lastName && Boolean(errors.lastName)}
                    helperText={touched.lastName && errors.lastName}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="email"
                    label="Email"
                    type="email"
                    value={values.email}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.email && Boolean(errors.email)}
                    helperText={touched.email && errors.email}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="password"
                    label="Password"
                    type="password"
                    value={values.password}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.password && Boolean(errors.password)}
                    helperText={touched.password && errors.password}
                    required={!isEditMode}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="phone"
                    label="Phone"
                    value={values.phone}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.phone && Boolean(errors.phone)}
                    helperText={touched.phone && errors.phone}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="gender"
                    label="Gender"
                    value={values.gender}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.gender && Boolean(errors.gender)}
                    helperText={touched.gender && errors.gender}
                  >
                    {GENDER_OPTIONS.map((option) => (
                      <MenuItem key={option} value={option}>
                        {option}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    name="address"
                    label="Address"
                    multiline
                    rows={2}
                    value={values.address}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
              </Grid>
            </Paper>

            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Employment Information
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="employeeId"
                    label="Employee ID"
                    value={values.employeeId}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.employeeId && Boolean(errors.employeeId)}
                    helperText={touched.employeeId && errors.employeeId}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="departmentName"
                    label="Department"
                    value={values.departmentName}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.departmentName && Boolean(errors.departmentName)}
                    helperText={touched.departmentName && errors.departmentName}
                  >
                    {DEPARTMENT_OPTIONS.map((option) => (
                      <MenuItem key={option} value={option}>
                        {option}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="dateOfJoining"
                    label="Date of Joining"
                    type="date"
                    value={values.dateOfJoining}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.dateOfJoining && Boolean(errors.dateOfJoining)}
                    helperText={touched.dateOfJoining && errors.dateOfJoining}
                    slotProps={{ inputLabel: { shrink: true } }}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="qualification"
                    label="Qualification"
                    value={values.qualification}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="experience"
                    label="Years of Experience"
                    type="number"
                    value={values.experience}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.experience && Boolean(errors.experience)}
                    helperText={touched.experience && errors.experience}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="status"
                    label="Status"
                    value={values.status}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  >
                    {STATUS_OPTIONS.map((option) => (
                      <MenuItem key={option} value={option}>
                        {option}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
              </Grid>
            </Paper>

            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button variant="outlined" onClick={() => navigate('/teachers')}>
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                startIcon={<SaveIcon />}
                disabled={isSubmitting}
              >
                {isSubmitting
                  ? 'Saving...'
                  : isEditMode
                  ? 'Update Teacher'
                  : 'Create Teacher'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
