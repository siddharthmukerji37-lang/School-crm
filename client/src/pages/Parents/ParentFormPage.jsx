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
import { createParent, updateParent, fetchParentById, clearSelectedParent } from '../../store/slices/parentSlice';
import toast from 'react-hot-toast';

const RELATIONSHIP_OPTIONS = ['Father', 'Mother', 'Guardian', 'Other'];

const parentSchema = Yup.object({
  firstName: Yup.string().trim().required('First name is required'),
  lastName: Yup.string().trim().required('Last name is required'),
  email: Yup.string().email('Invalid email').required('Email is required'),
  phone: Yup.string().matches(/^[0-9+\-\s()]*$/, 'Invalid phone number'),
  occupation: Yup.string().trim(),
  relationship: Yup.string().oneOf(['Father', 'Mother', 'Guardian', 'Other']).required('Relationship is required'),
  address: Yup.string().trim(),
});

export default function ParentFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedParent, loading } = useSelector((state) => state.parents);
  const isEditMode = Boolean(id);

  const [initialValues, setInitialValues] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    occupation: '',
    relationship: '',
    address: '',
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
    if (isEditMode && selectedParent) {
      setInitialValues({
        firstName: selectedParent.firstName || '',
        lastName: selectedParent.lastName || '',
        email: selectedParent.email || '',
        phone: selectedParent.phone || '',
        occupation: selectedParent.occupation || '',
        relationship: selectedParent.relationship || '',
        address: selectedParent.address || '',
      });
    }
  }, [isEditMode, selectedParent]);

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      if (isEditMode) {
        const result = await dispatch(updateParent({ id, data: values }));
        if (updateParent.fulfilled.match(result)) {
          toast.success('Parent updated successfully');
          navigate('/parents');
        } else {
          toast.error(result.payload || 'Failed to update parent');
        }
      } else {
        const result = await dispatch(createParent(values));
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
                <Grid size={{ xs: 12, sm: 6 }}>
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
              </Grid>
            </Paper>

            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Contact Information
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
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
              </Grid>
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
