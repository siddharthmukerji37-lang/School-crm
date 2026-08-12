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
  FormControlLabel,
  Switch,
  Divider,
  Stack,
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { createStudent, updateStudent, fetchStudentById, clearSelectedStudent } from '../../store/slices/studentSlice';
import { studentSchema, studentEditSchema } from '../../validationSchemas/studentSchemas';
import toast from 'react-hot-toast';
import axiosInstance from '../../services/axiosInstance';

const GENDER_OPTIONS = ['Male', 'Female', 'Other'];
const BLOOD_GROUP_OPTIONS = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];

const toDateInputValue = (value) => {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

export default function StudentFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedStudent, loading } = useSelector((state) => state.students);
  const isEditMode = Boolean(id);

  const [classes, setClasses] = useState([]);
  const [sections, setSections] = useState([]);
  const [classesLoading, setClassesLoading] = useState(true);

  const [initialValues, setInitialValues] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    dateOfBirth: '',
    gender: '',
    classRoomId: '',
    sectionId: '',
    admissionNumber: '',
    admissionDate: '',
    phone: '',
    address: '',
    parentName: '',
    parentPhone: '',
    parentEmail: '',
    bloodGroup: '',
    transportRequired: false,
    hostelRequired: false,
    notes: '',
  });

  useEffect(() => {
    const fetchClasses = async () => {
      try {
        const res = await axiosInstance.get('/schools');
        const schools = res.data.data?.items || res.data.data || [];
        if (schools.length > 0) {
          const schoolId = schools[0].id;
          const classRes = await axiosInstance.get(`/schools/${schoolId}/classes`);
          setClasses(classRes.data.data || []);
        }
      } catch {
        setClasses([]);
      } finally {
        setClassesLoading(false);
      }
    };
    fetchClasses();
  }, []);

  useEffect(() => {
    if (isEditMode) {
      dispatch(fetchStudentById(id));
    }
    return () => {
      dispatch(clearSelectedStudent());
    };
  }, [dispatch, id, isEditMode]);

  useEffect(() => {
    if (isEditMode && selectedStudent) {
      setInitialValues({
        firstName: selectedStudent.firstName || '',
        lastName: selectedStudent.lastName || '',
        email: selectedStudent.email || '',
        dateOfBirth: toDateInputValue(selectedStudent.dateOfBirth),
        gender: selectedStudent.gender || '',
        classRoomId: selectedStudent.classRoomId || '',
        sectionId: selectedStudent.sectionId || '',
        admissionNumber: selectedStudent.admissionNumber || '',
        admissionDate: toDateInputValue(selectedStudent.admissionDate),
        phone: selectedStudent.phone || '',
        address: selectedStudent.address || '',
        parentName: selectedStudent.parentName || '',
        parentPhone: selectedStudent.parentPhone || '',
        parentEmail: selectedStudent.parentEmail || '',
        bloodGroup: selectedStudent.bloodGroup || '',
        transportRequired: selectedStudent.transportRequired || false,
        hostelRequired: selectedStudent.hostelRequired || false,
        notes: selectedStudent.notes || '',
      });
    }
  }, [isEditMode, selectedStudent]);

  const fetchSections = async (classRoomId) => {
    if (!classRoomId) {
      setSections([]);
      return;
    }
    try {
      const res = await axiosInstance.get(`/schools/classes/${classRoomId}/sections`);
      setSections(res.data.data || []);
    } catch {
      setSections([]);
    }
  };

  useEffect(() => {
    if (isEditMode && selectedStudent?.classRoomId) {
      fetchSections(selectedStudent.classRoomId);
    }
  }, [isEditMode, selectedStudent?.classRoomId]);

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      const payload = {
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        password: isEditMode ? undefined : values.password,
        phone: values.phone,
        gender: values.gender,
        dateOfBirth: values.dateOfBirth,
        sectionId: values.sectionId,
        classRoomId: values.classRoomId,
        admissionNumber: values.admissionNumber || null,
        admissionDate: values.admissionDate,
        address: values.address,
        bloodGroup: values.bloodGroup || null,
        parentName: values.parentName,
        parentPhone: values.parentPhone,
        parentEmail: values.parentEmail,
        transportRequired: Boolean(values.transportRequired),
        hostelRequired: Boolean(values.hostelRequired),
        notes: values.notes,
      };
      if (isEditMode) {
        payload.status = selectedStudent?.status || 'Active';
        const result = await dispatch(updateStudent({ id, data: payload }));
        if (updateStudent.fulfilled.match(result)) {
          toast.success('Student updated successfully');
          navigate(`/students/${id}`);
        } else {
          toast.error(result.payload || 'Failed to update student');
        }
      } else {
        const result = await dispatch(createStudent(payload));
        if (createStudent.fulfilled.match(result)) {
          toast.success('Student created successfully');
          navigate('/students');
        } else {
          toast.error(result.payload || 'Failed to create student');
        }
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (isEditMode && loading && !selectedStudent) {
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
          onClick={() => navigate(isEditMode ? `/students/${id}` : '/students')}
          variant="outlined"
        >
          Back
        </Button>
        <Typography variant="h4" fontWeight={700}>
          {isEditMode ? 'Edit Student' : 'Add New Student'}
        </Typography>
      </Box>

      <Formik
        initialValues={initialValues}
        validationSchema={isEditMode ? studentEditSchema : studentSchema}
        onSubmit={handleSubmit}
        enableReinitialize
      >
        {({
          values,
          errors,
          touched,
          handleChange,
          handleBlur,
          setFieldValue,
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
                {!isEditMode && (
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
                      helperText={(touched.password && errors.password) || 'Student login password'}
                    />
                  </Grid>
                )}
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="dateOfBirth"
                    label="Date of Birth"
                    type="date"
                    value={values.dateOfBirth}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.dateOfBirth && Boolean(errors.dateOfBirth)}
                    helperText={touched.dateOfBirth && errors.dateOfBirth}
                    slotProps={{ inputLabel: { shrink: true } }}
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
                    select
                    name="bloodGroup"
                    label="Blood Group"
                    value={values.bloodGroup}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  >
                    <MenuItem value="">Select Blood Group</MenuItem>
                    {BLOOD_GROUP_OPTIONS.map((option) => (
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
                Academic Information
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="admissionNumber"
                    label="Admission Number"
                    value={values.admissionNumber}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.admissionNumber && Boolean(errors.admissionNumber)}
                    helperText={touched.admissionNumber && errors.admissionNumber}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="admissionDate"
                    label="Admission Date"
                    type="date"
                    value={values.admissionDate}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.admissionDate && Boolean(errors.admissionDate)}
                    helperText={touched.admissionDate && errors.admissionDate}
                    slotProps={{ inputLabel: { shrink: true } }}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="classRoomId"
                    label="Class"
                    value={values.classRoomId}
                    onChange={(e) => {
                      handleChange(e);
                      setFieldValue('sectionId', '');
                      fetchSections(e.target.value);
                    }}
                    onBlur={handleBlur}
                    error={touched.classRoomId && Boolean(errors.classRoomId)}
                    helperText={touched.classRoomId && errors.classRoomId}
                    disabled={classesLoading}
                  >
                    {classes.map((cls) => (
                      <MenuItem key={cls.id} value={cls.id}>
                        {cls.name}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="sectionId"
                    label="Section"
                    value={values.sectionId}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.sectionId && Boolean(errors.sectionId)}
                    helperText={touched.sectionId && errors.sectionId}
                    disabled={!values.classRoomId}
                  >
                    {sections.map((sec) => (
                      <MenuItem key={sec.id} value={sec.id}>
                        {sec.name}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
              </Grid>
            </Paper>

            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Parent / Guardian Information
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="parentName"
                    label="Parent/Guardian Name"
                    value={values.parentName}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.parentName && Boolean(errors.parentName)}
                    helperText={touched.parentName && errors.parentName}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="parentPhone"
                    label="Parent Phone"
                    value={values.parentPhone}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.parentPhone && Boolean(errors.parentPhone)}
                    helperText={touched.parentPhone && errors.parentPhone}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="parentEmail"
                    label="Parent Email"
                    type="email"
                    value={values.parentEmail}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.parentEmail && Boolean(errors.parentEmail)}
                    helperText={touched.parentEmail && errors.parentEmail}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="phone"
                    label="Student Phone"
                    value={values.phone}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.phone && Boolean(errors.phone)}
                    helperText={touched.phone && errors.phone}
                  />
                </Grid>
              </Grid>
            </Paper>

            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Additional Information
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={values.transportRequired}
                        onChange={(e) =>
                          setFieldValue('transportRequired', e.target.checked)
                        }
                        color="primary"
                      />
                    }
                    label="Transport Required"
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={values.hostelRequired}
                        onChange={(e) =>
                          setFieldValue('hostelRequired', e.target.checked)
                        }
                        color="primary"
                      />
                    }
                    label="Hostel Required"
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    name="notes"
                    label="Notes"
                    multiline
                    rows={3}
                    value={values.notes}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
              </Grid>
            </Paper>

            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button
                variant="outlined"
                onClick={() =>
                  navigate(isEditMode ? `/students/${id}` : '/students')
                }
              >
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
                  ? 'Update Student'
                  : 'Create Student'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
