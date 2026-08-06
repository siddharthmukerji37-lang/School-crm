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
import { createEmployee, updateEmployee, fetchEmployeeById, clearSelectedEmployee } from '../../store/slices/employeeSlice';
import toast from 'react-hot-toast';

const GENDER_OPTIONS = ['Male', 'Female', 'Other'];
const EMPLOYEE_TYPE_OPTIONS = ['FullTime', 'PartTime', 'Contract', 'Intern', 'Temporary'];
const STATUS_OPTIONS = ['Active', 'OnLeave', 'Inactive'];
const DEPARTMENT_OPTIONS = [
  'Administration',
  'Mathematics',
  'Science',
  'English',
  'History',
  'Geography',
  'Computer Science',
  'Arts',
  'Physical Education',
  'Finance',
  'HR',
  'IT Support',
];

const createEmployeeSchema = Yup.object({
  firstName: Yup.string().trim().required('First name is required'),
  lastName: Yup.string().trim().required('Last name is required'),
  email: Yup.string().email('Invalid email').required('Email is required'),
  phone: Yup.string().matches(/^[0-9+\-\s()]*$/, 'Invalid phone number'),
  employeeId: Yup.string().trim().required('Employee ID is required'),
  department: Yup.string().required('Department is required'),
  gender: Yup.string().oneOf(['Male', 'Female', 'Other']).required('Gender is required'),
  joiningDate: Yup.date().nullable().required('Date of joining is required'),
  designation: Yup.string().trim(),
  address: Yup.string().trim(),
  password: Yup.string()
    .min(6, 'Password must be at least 6 characters'),
});

const updateEmployeeSchema = Yup.object({
  firstName: Yup.string().trim().required('First name is required'),
  lastName: Yup.string().trim().required('Last name is required'),
  email: Yup.string().email('Invalid email').required('Email is required'),
  phone: Yup.string().matches(/^[0-9+\-\s()]*$/, 'Invalid phone number'),
  employeeId: Yup.string().trim().required('Employee ID is required'),
  department: Yup.string().required('Department is required'),
  gender: Yup.string().oneOf(['Male', 'Female', 'Other']).required('Gender is required'),
  joiningDate: Yup.date().nullable().required('Date of joining is required'),
  designation: Yup.string().trim(),
  address: Yup.string().trim(),
  password: Yup.string().min(8, 'Password must be at least 8 characters'),
});

export default function EmployeeFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedEmployee, loading } = useSelector((state) => state.employees);
  const isEditMode = Boolean(id);

  const [initialValues, setInitialValues] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    employeeId: '',
    department: '',
    gender: '',
    joiningDate: '',
    designation: '',
    employeeType: 'FullTime',
    status: 'Active',
    address: '',
    password: '',
  });

  useEffect(() => {
    if (isEditMode) {
      dispatch(fetchEmployeeById(id));
    }
    return () => {
      dispatch(clearSelectedEmployee());
    };
  }, [dispatch, id, isEditMode]);

  useEffect(() => {
    if (isEditMode && selectedEmployee) {
      setInitialValues({
        firstName: selectedEmployee.firstName || '',
        lastName: selectedEmployee.lastName || '',
        email: selectedEmployee.email || '',
        phone: selectedEmployee.phone || '',
        employeeId: selectedEmployee.employeeId || '',
        department: selectedEmployee.department || '',
        gender: selectedEmployee.gender || '',
        joiningDate: selectedEmployee.joiningDate
          ? new Date(selectedEmployee.joiningDate).toISOString().split('T')[0]
          : '',
        designation: selectedEmployee.designation || '',
        employeeType: selectedEmployee.employeeType || 'FullTime',
        status: selectedEmployee.status || 'Active',
        address: selectedEmployee.address || '',
        password: '',
      });
    }
  }, [isEditMode, selectedEmployee]);

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      if (isEditMode) {
        const { password, ...rest } = values;
        const data = password ? values : rest;
        const result = await dispatch(updateEmployee({ id, data }));
        if (updateEmployee.fulfilled.match(result)) {
          toast.success('Employee updated successfully');
          navigate('/employees');
        } else {
          toast.error(result.payload || 'Failed to update employee');
        }
      } else {
        const result = await dispatch(createEmployee(values));
        if (createEmployee.fulfilled.match(result)) {
          toast.success('Employee created successfully');
          navigate('/employees');
        } else {
          toast.error(result.payload || 'Failed to create employee');
        }
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (isEditMode && loading && !selectedEmployee) {
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
          onClick={() => navigate('/employees')}
          variant="outlined"
        >
          Back
        </Button>
        <Typography variant="h4" fontWeight={700}>
          {isEditMode ? 'Edit Employee' : 'Add New Employee'}
        </Typography>
      </Box>

      <Formik
        initialValues={initialValues}
        validationSchema={isEditMode ? updateEmployeeSchema : createEmployeeSchema}
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
                    name="department"
                    label="Department"
                    value={values.department}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.department && Boolean(errors.department)}
                    helperText={touched.department && errors.department}
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
                    name="designation"
                    label="Designation"
                    value={values.designation}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="employeeType"
                    label="Employee Type"
                    value={values.employeeType}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.employeeType && Boolean(errors.employeeType)}
                    helperText={touched.employeeType && errors.employeeType}
                  >
                    {EMPLOYEE_TYPE_OPTIONS.map((option) => (
                      <MenuItem key={option} value={option}>
                        {option}
                      </MenuItem>
                    ))}
                  </TextField>
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
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="joiningDate"
                    label="Date of Joining"
                    type="date"
                    value={values.joiningDate}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.joiningDate && Boolean(errors.joiningDate)}
                    helperText={touched.joiningDate && errors.joiningDate}
                    slotProps={{ inputLabel: { shrink: true } }}
                  />
                </Grid>
              </Grid>
            </Paper>

            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button variant="outlined" onClick={() => navigate('/employees')}>
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
                  ? 'Update Employee'
                  : 'Create Employee'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
