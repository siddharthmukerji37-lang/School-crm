import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Formik, Form } from 'formik';
import {
  Box,
  Grid,
  TextField,
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
import {
  createVendor,
  updateVendor,
  fetchVendors,
} from '../../store/slices/inventorySlice';
import toast from 'react-hot-toast';

const vendorSchema = Yup.object({
  name: Yup.string().trim().required('Name is required'),
  contactPerson: Yup.string().trim(),
  phone: Yup.string().trim(),
  email: Yup.string().trim().email('Enter a valid email'),
  address: Yup.string().trim(),
  gstNumber: Yup.string().trim(),
});

export default function VendorFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { vendors, loading } = useSelector((state) => state.inventory);
  const isEditMode = Boolean(id);

  const [initialValues, setInitialValues] = useState({
    name: '',
    contactPerson: '',
    phone: '',
    email: '',
    address: '',
    gstNumber: '',
  });

  useEffect(() => {
    dispatch(fetchVendors({ pageSize: 1000 }));
  }, [dispatch]);

  useEffect(() => {
    if (isEditMode && vendors.items?.length > 0) {
      const found = vendors.items.find((vendor) => vendor.id === id);
      if (found) {
        setInitialValues({
          name: found.name || '',
          contactPerson: found.contactPerson || '',
          phone: found.phone || '',
          email: found.email || '',
          address: found.address || '',
          gstNumber: found.gstNumber || '',
        });
      }
    }
  }, [isEditMode, id, vendors.items]);

  const handleSubmit = async (values, { setSubmitting }) => {
    const payload = {
      ...values,
      contactPerson: values.contactPerson || null,
      phone: values.phone || '',
      email: values.email || null,
      address: values.address || null,
      gstNumber: values.gstNumber || null,
    };

    const result = isEditMode
      ? await dispatch(updateVendor({ id, data: payload }))
      : await dispatch(createVendor(payload));

    const thunk = isEditMode ? updateVendor : createVendor;
    if (thunk.fulfilled.match(result)) {
      toast.success(
        isEditMode ? 'Vendor updated successfully' : 'Vendor created successfully'
      );
      navigate('/inventory');
    } else {
      toast.error(result.payload || 'Failed to save vendor');
    }
    setSubmitting(false);
  };

  if (isEditMode && loading && !initialValues.name) {
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
          onClick={() => navigate('/inventory')}
          variant="outlined"
        >
          Back
        </Button>
        <Typography variant="h4" fontWeight={700}>
          {isEditMode ? 'Edit Vendor' : 'Add New Vendor'}
        </Typography>
      </Box>

      <Formik
        initialValues={initialValues}
        validationSchema={vendorSchema}
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
                Vendor Details
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="name"
                    label="Vendor Name"
                    value={values.name}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.name && Boolean(errors.name)}
                    helperText={touched.name && errors.name}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="contactPerson"
                    label="Contact Person"
                    value={values.contactPerson}
                    onChange={handleChange}
                    onBlur={handleBlur}
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
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="email"
                    label="Email"
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
                    name="address"
                    label="Address"
                    multiline
                    rows={2}
                    value={values.address}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="gstNumber"
                    label="GST Number"
                    value={values.gstNumber}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
              </Grid>
            </Paper>

            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button variant="outlined" onClick={() => navigate('/inventory')}>
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
                  ? 'Update Vendor'
                  : 'Create Vendor'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
