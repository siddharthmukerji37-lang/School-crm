import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Formik, Form } from 'formik';
import { Autocomplete, Box, TextField, MenuItem, Button, Paper, Typography, CircularProgress, Divider, Stack, Checkbox, Avatar } from '@mui/material';
import Grid from '@mui/material/Grid2';
import CheckBoxOutlineBlankIcon from '@mui/icons-material/CheckBoxOutlineBlank';
import CheckBoxIcon from '@mui/icons-material/CheckBox';
import SaveIcon from '@mui/icons-material/Save';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import CameraAltIcon from '@mui/icons-material/CameraAlt';
import * as Yup from 'yup';
import { createParent, updateParent, fetchParentById, clearSelectedParent } from '../../store/slices/parentSlice';
import { fetchStudents } from '../../store/slices/studentSlice';
import toast from 'react-hot-toast';
import { uploadFile } from '../../utils/upload';

const RELATIONSHIP_OPTIONS = ['Father', 'Mother', 'Guardian', 'Other'];

const icon = <CheckBoxOutlineBlankIcon fontSize="small" />;
const checkedIcon = <CheckBoxIcon fontSize="small" />;

const parentSchema = Yup.object({
  firstName: Yup.string().trim().required('First name is required'),
  lastName: Yup.string().trim().required('Last name is required'),
  email: Yup.string().email('Invalid email').required('Email is required'),
  phone: Yup.string().matches(/^[0-9+\-\s()]*$/, 'Invalid phone number'),
  occupation: Yup.string().trim(),
  relationship: Yup.string().oneOf(['Father', 'Mother', 'Guardian', 'Other']).required('Relationship is required'),
  address: Yup.string().trim(),
  city: Yup.string().trim(),
  state: Yup.string().trim(),
  country: Yup.string().trim(),
  postalCode: Yup.string().trim(),
});

export default function ParentFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedParent, loading } = useSelector((state) => state.parents);
  const { students, loading: studentsLoading } = useSelector((state) => state.students);
  const isEditMode = Boolean(id);

  const [initialValues, setInitialValues] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    occupation: '',
    relationship: '',
    address: '',
    city: '',
    state: '',
    country: '',
    postalCode: '',
    childrenStudentIds: [],
    profilePictureUrl: '',
  });

  useEffect(() => {
    if (isEditMode) {
      dispatch(fetchParentById(id));
    }
    return () => {
      dispatch(clearSelectedParent());
    };
  }, [dispatch, id, isEditMode]);

  useEffect(() => {
    dispatch(fetchStudents({ page: 1, pageSize: 200 }));
  }, [dispatch]);

  useEffect(() => {
    if (isEditMode && selectedParent) {
      setInitialValues({
        firstName: selectedParent.firstName || '',
        lastName: selectedParent.lastName || '',
        email: selectedParent.email || '',
        phone: selectedParent.phone || '',
        occupation: selectedParent.occupation || '',
        relationship: selectedParent.relationship || '',
        address: selectedParent.address || '',
        city: selectedParent.city || '',
        state: selectedParent.state || '',
        country: selectedParent.country || '',
        postalCode: selectedParent.postalCode || '',
        childrenStudentIds: (selectedParent.children || []).map((c) => c.studentId),
        profilePictureUrl: selectedParent.profilePictureUrl || '',
      });
    }
  }, [isEditMode, selectedParent]);

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      const payload = {
        ...values,
        childrenStudentIds: values.childrenStudentIds || [],
      };
      if (isEditMode) {
        const result = await dispatch(updateParent({ id, data: payload }));
        if (updateParent.fulfilled.match(result)) {
          toast.success('Parent updated successfully');
          navigate('/parents');
        } else {
          toast.error(result.payload || 'Failed to update parent');
        }
      } else {
        const result = await dispatch(createParent(payload));
        if (createParent.fulfilled.match(result)) {
          toast.success('Parent created successfully');
          navigate('/parents');
        } else {
          toast.error(result.payload || 'Failed to create parent');
        }
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (isEditMode && loading && !selectedParent) {
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
          onClick={() => navigate('/parents')}
          variant="outlined"
        >
          Back
        </Button>
        <Typography variant="h4" fontWeight={700}>
          {isEditMode ? 'Edit Parent' : 'Add New Parent'}
        </Typography>
      </Box>

      <Formik
        initialValues={initialValues}
        validationSchema={parentSchema}
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
                Profile Photo
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 3 }}>
                <Box sx={{ position: 'relative' }}>
                  <Avatar
                    src={values.profilePictureUrl || undefined}
                    sx={{ width: 96, height: 96, bgcolor: 'primary.main', fontSize: '2rem', cursor: 'pointer' }}
                    onClick={() => document.getElementById('parent-photo-input').click()}
                  >
                    {values.firstName?.charAt(0)}{values.lastName?.charAt(0)}
                  </Avatar>
                  <Box
                    sx={{
                      position: 'absolute', bottom: 0, right: 0, bgcolor: 'primary.main',
                      borderRadius: '50%', width: 28, height: 28, display: 'flex',
                      alignItems: 'center', justifyContent: 'center', cursor: 'pointer',
                    }}
                    onClick={() => document.getElementById('parent-photo-input').click()}
                  >
                    <CameraAltIcon sx={{ fontSize: 16, color: 'white' }} />
                  </Box>
                </Box>
                <Box>
                  <Typography variant="body2" color="text.secondary">
                    Click the photo or camera icon to upload
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    JPG, PNG or WebP. Max 10MB.
                  </Typography>
                </Box>
                <input
                  id="parent-photo-input"
                  type="file"
                  hidden
                  accept="image/*"
                  onChange={async (e) => {
                    const file = e.target.files?.[0];
                    if (!file) return;
                    try {
                      const res = await uploadFile(file);
                      setFieldValue('profilePictureUrl', res.url);
                      toast.success('Photo uploaded');
                    } catch {
                      toast.error('Failed to upload photo');
                    }
                  }}
                />
              </Box>
            </Paper>

            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Personal Information
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12 }}>
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
                <Grid size={{ xs: 12 }}>
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
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    select
                    name="relationship"
                    label="Relationship"
                    value={values.relationship}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.relationship && Boolean(errors.relationship)}
                    helperText={touched.relationship && errors.relationship}
                  >
                    {RELATIONSHIP_OPTIONS.map((option) => (
                      <MenuItem key={option} value={option}>
                        {option}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    name="occupation"
                    label="Occupation"
                    value={values.occupation}
                    onChange={handleChange}
                    onBlur={handleBlur}
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
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    name="city"
                    label="City"
                    value={values.city}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    name="state"
                    label="State"
                    value={values.state}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    name="country"
                    label="Country"
                    value={values.country}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    name="postalCode"
                    label="Pin Code"
                    value={values.postalCode}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
              </Grid>
            </Paper>

            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Contact Information
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12 }}>
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
                <Grid size={{ xs: 12 }}>
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
              </Grid>
            </Paper>

            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Children
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                Select the students linked to this parent. A student can be linked to multiple parents.
              </Typography>
              <Autocomplete
                multiple
                fullWidth
                options={students.items || []}
                loading={studentsLoading}
                value={(students.items || []).filter((s) =>
                  (values.childrenStudentIds || []).includes(s.id)
                )}
                onChange={(_, value) =>
                  setFieldValue('childrenStudentIds', (value || []).map((s) => s.id))
                }
                getOptionLabel={(option) =>
                  `${option.firstName || ''} ${option.lastName || ''}${option.admissionNumber ? ` (${option.admissionNumber})` : ''}`
                }
                disableCloseOnSelect
                renderOption={(props, option, { selected }) => (
                  <li {...props}>
                    <Checkbox
                      icon={icon}
                      checkedIcon={checkedIcon}
                      checked={selected}
                      sx={{ mr: 1 }}
                    />
                    {`${option.firstName || ''} ${option.lastName || ''}`}
                    {option.admissionNumber ? ` (${option.admissionNumber})` : ''}
                    {option.className ? ` - ${option.className}${option.sectionName ? `, ${option.sectionName}` : ''}` : ''}
                  </li>
                )}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Linked Students"
                    placeholder="Select students"
                  />
                )}
              />
            </Paper>

            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button variant="outlined" onClick={() => navigate('/parents')}>
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
                  ? 'Update Parent'
                  : 'Create Parent'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
