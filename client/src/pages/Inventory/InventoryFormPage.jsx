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
import {
  createItem,
  updateItem,
  fetchItems,
  fetchVendors,
} from '../../store/slices/inventorySlice';
import toast from 'react-hot-toast';

const CATEGORY_OPTIONS = [
  'Furniture',
  'Electronics',
  'Stationery',
  'Sports',
  'Lab Equipment',
  'Other',
];

const UNIT_OPTIONS = ['Piece', 'Kg', 'Liter', 'Box', 'Set'];

const itemSchema = Yup.object({
  name: Yup.string().trim().required('Name is required'),
  category: Yup.string().required('Category is required'),
  description: Yup.string().trim(),
  quantity: Yup.number()
    .transform((value, originalValue) =>
      originalValue === '' ? undefined : value
    )
    .required('Quantity is required')
    .min(0, 'Must be zero or positive')
    .integer('Must be a whole number'),
  unit: Yup.string().required('Unit is required'),
  reorderLevel: Yup.number()
    .transform((value, originalValue) =>
      originalValue === '' ? undefined : value
    )
    .required('Reorder level is required')
    .min(0, 'Must be zero or positive')
    .integer('Must be a whole number'),
  purchasePrice: Yup.number()
    .transform((value, originalValue) =>
      originalValue === '' ? undefined : value
    )
    .min(0, 'Must be positive'),
  vendorId: Yup.string().nullable(),
});

export default function InventoryFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { items, vendors, loading } = useSelector((state) => state.inventory);
  const isEditMode = Boolean(id);

  const [initialValues, setInitialValues] = useState({
    name: '',
    category: '',
    description: '',
    quantity: '',
    unit: '',
    reorderLevel: '',
    purchasePrice: '',
    vendorId: '',
  });

  useEffect(() => {
    if (!isEditMode) {
      dispatch(fetchItems());
    }
    dispatch(fetchVendors({ pageSize: 1000 }));
  }, [dispatch, isEditMode]);

  useEffect(() => {
    if (isEditMode && items.items?.length > 0) {
      const found = items.items.find((item) => item.id === id);
      if (found) {
        setInitialValues({
          name: found.name || '',
          category: found.category || '',
          description: found.description || '',
          quantity: found.quantity ?? '',
          unit: found.unit || '',
          reorderLevel: found.reorderLevel ?? '',
          purchasePrice: found.purchasePrice ?? '',
          vendorId: found.vendorId || '',
        });
      }
    }
  }, [isEditMode, id, items.items]);

  const handleSubmit = async (values, { setSubmitting }) => {
    const payload = {
      ...values,
      quantity: values.quantity !== '' ? Number(values.quantity) : 0,
      minimumStock: values.reorderLevel !== '' ? Number(values.reorderLevel) : 0,
      purchasePrice: values.purchasePrice !== '' ? Number(values.purchasePrice) : null,
      vendorId: values.vendorId || null,
    };

    const result = isEditMode
      ? await dispatch(updateItem({ id, data: payload }))
      : await dispatch(createItem(payload));

    const thunk = isEditMode ? updateItem : createItem;
    if (thunk.fulfilled.match(result)) {
      toast.success(isEditMode ? 'Item updated successfully' : 'Item created successfully');
      navigate('/inventory');
    } else {
      toast.error(result.payload || 'Failed to save item');
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
          {isEditMode ? 'Edit Item' : 'Add New Item'}
        </Typography>
      </Box>

      <Formik
        initialValues={initialValues}
        validationSchema={itemSchema}
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
                Item Details
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="name"
                    label="Item Name"
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
                    select
                    name="category"
                    label="Category"
                    value={values.category}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.category && Boolean(errors.category)}
                    helperText={touched.category && errors.category}
                  >
                    {CATEGORY_OPTIONS.map((option) => (
                      <MenuItem key={option} value={option}>
                        {option}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    name="description"
                    label="Description"
                    multiline
                    rows={2}
                    value={values.description}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="vendorId"
                    label="Vendor"
                    value={values.vendorId}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  >
                    <MenuItem value="">
                      <em>None</em>
                    </MenuItem>
                    {(vendors.items || []).map((vendor) => (
                      <MenuItem key={vendor.id} value={vendor.id}>
                        {vendor.name}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="purchasePrice"
                    label="Purchase Price"
                    type="number"
                    value={values.purchasePrice}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.purchasePrice && Boolean(errors.purchasePrice)}
                    helperText={touched.purchasePrice && errors.purchasePrice}
                  />
                </Grid>
              </Grid>
            </Paper>

            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Stock Information
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField
                    fullWidth
                    name="quantity"
                    label="Quantity"
                    type="number"
                    value={values.quantity}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.quantity && Boolean(errors.quantity)}
                    helperText={touched.quantity && errors.quantity}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField
                    fullWidth
                    select
                    name="unit"
                    label="Unit"
                    value={values.unit}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.unit && Boolean(errors.unit)}
                    helperText={touched.unit && errors.unit}
                  >
                    {UNIT_OPTIONS.map((option) => (
                      <MenuItem key={option} value={option}>
                        {option}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField
                    fullWidth
                    name="reorderLevel"
                    label="Reorder Level"
                    type="number"
                    value={values.reorderLevel}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.reorderLevel && Boolean(errors.reorderLevel)}
                    helperText={touched.reorderLevel && errors.reorderLevel}
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
                  ? 'Update Item'
                  : 'Create Item'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
