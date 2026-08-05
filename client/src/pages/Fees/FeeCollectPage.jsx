import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch } from 'react-redux';
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
  Chip,
  Autocomplete,
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import * as Yup from 'yup';
import { collectFee } from '../../store/slices/feeSlice';
import axiosInstance from '../../services/axiosInstance';
import toast from 'react-hot-toast';

const PAYMENT_METHODS = ['Cash', 'Bank Transfer', 'Cheque', 'Online'];

const collectFeeSchema = Yup.object({
  studentId: Yup.string().required('Student is required'),
  feeStructureId: Yup.string().required('Fee structure is required'),
  amount: Yup.number()
    .transform((value, originalValue) =>
      originalValue === '' ? undefined : value
    )
    .required('Amount is required')
    .min(0.01, 'Amount must be greater than 0'),
  paymentMethod: Yup.string()
    .oneOf(['Cash', 'Bank Transfer', 'Cheque', 'Online'])
    .required('Payment method is required'),
  transactionReference: Yup.string().trim(),
  remarks: Yup.string().trim(),
  paymentDate: Yup.date().required('Payment date is required'),
});

export default function FeeCollectPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();

  const [students, setStudents] = useState([]);
  const [studentsLoading, setStudentsLoading] = useState(false);
  const [feeStructures, setFeeStructures] = useState([]);
  const [feesLoading, setFeesLoading] = useState(false);
  const [pendingFees, setPendingFees] = useState(null);
  const [pendingLoading, setPendingLoading] = useState(false);

  useEffect(() => {
    const loadStudents = async () => {
      setStudentsLoading(true);
      try {
        const response = await axiosInstance.get('/students', {
          params: { pageSize: 1000 },
        });
        setStudents(response.data.data?.items || []);
      } catch (error) {
        toast.error('Failed to load students');
      } finally {
        setStudentsLoading(false);
      }
    };
    loadStudents();
  }, []);

  useEffect(() => {
    const loadFeeStructures = async () => {
      setFeesLoading(true);
      try {
        const response = await axiosInstance.get('/fees', {
          params: { pageSize: 1000, status: 'Active' },
        });
        setFeeStructures(response.data.data?.items || []);
      } catch (error) {
        toast.error('Failed to load fee structures');
      } finally {
        setFeesLoading(false);
      }
    };
    loadFeeStructures();
  }, []);

  const handleStudentChange = async (studentId) => {
    if (!studentId) {
      setPendingFees(null);
      return;
    }
    setPendingLoading(true);
    try {
      const response = await axiosInstance.get('/fees/pending', {
        params: { studentId },
      });
      setPendingFees(response.data.data);
    } catch {
      setPendingFees(null);
    } finally {
      setPendingLoading(false);
    }
  };

  const handleSubmit = async (values, { setSubmitting }) => {
    const result = await dispatch(collectFee(values));
    if (collectFee.fulfilled.match(result)) {
      toast.success('Fee collected successfully');
      navigate('/fees');
    } else {
      toast.error(result.payload || 'Failed to collect fee');
    }
    setSubmitting(false);
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/fees')}
          variant="outlined"
        >
          Back
        </Button>
        <Typography variant="h4" fontWeight={700}>
          Collect Fee
        </Typography>
      </Box>

      <Formik
        initialValues={{
          studentId: '',
          feeStructureId: '',
          amount: '',
          paymentMethod: 'Cash',
          transactionReference: '',
          remarks: '',
          paymentDate: new Date().toISOString().split('T')[0],
        }}
        validationSchema={collectFeeSchema}
        onSubmit={handleSubmit}
      >
        {({
          values,
          errors,
          touched,
          handleChange,
          handleBlur,
          isSubmitting,
          setFieldValue,
        }) => (
          <Form>
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Student & Fee Details
              </Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <Autocomplete
                    options={students}
                    getOptionLabel={(option) =>
                      `${option.firstName || ''} ${option.lastName || ''} (${option.admissionNumber || ''})`
                    }
                    loading={studentsLoading}
                    onChange={(_, newValue) => {
                      setFieldValue('studentId', newValue?.id || '');
                      handleStudentChange(newValue?.id);
                    }}
                    renderInput={(params) => (
                      <TextField
                        {...params}
                        label="Student"
                        placeholder="Search student..."
                        error={touched.studentId && Boolean(errors.studentId)}
                        helperText={touched.studentId && errors.studentId}
                        InputProps={{
                          ...params.InputProps,
                          endAdornment: (
                            <>
                              {studentsLoading ? (
                                <CircularProgress color="inherit" size={16} />
                              ) : null}
                              {params.InputProps.endAdornment}
                            </>
                          ),
                        }}
                      />
                    )}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="feeStructureId"
                    label="Fee Structure"
                    value={values.feeStructureId}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.feeStructureId && Boolean(errors.feeStructureId)}
                    helperText={touched.feeStructureId && errors.feeStructureId}
                    disabled={feesLoading}
                  >
                    {feeStructures.map((fs) => (
                      <MenuItem key={fs.id} value={fs.id}>
                        {fs.name} - {fs.className} (${Number(fs.totalAmount || 0).toFixed(2)})
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="amount"
                    label="Amount"
                    type="number"
                    value={values.amount}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.amount && Boolean(errors.amount)}
                    helperText={touched.amount && errors.amount}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="paymentMethod"
                    label="Payment Method"
                    value={values.paymentMethod}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.paymentMethod && Boolean(errors.paymentMethod)}
                    helperText={touched.paymentMethod && errors.paymentMethod}
                  >
                    {PAYMENT_METHODS.map((option) => (
                      <MenuItem key={option} value={option}>
                        {option}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="transactionReference"
                    label="Transaction Reference"
                    value={values.transactionReference}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="paymentDate"
                    label="Payment Date"
                    type="date"
                    value={values.paymentDate}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    slotProps={{ inputLabel: { shrink: true } }}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="remarks"
                    label="Remarks"
                    value={values.remarks}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
              </Grid>
            </Paper>

            {pendingLoading && (
              <Paper sx={{ p: 3, mb: 3, textAlign: 'center' }}>
                <CircularProgress size={24} />
                <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                  Loading pending fees...
                </Typography>
              </Paper>
            )}

            {!pendingLoading && pendingFees && (
              <Paper sx={{ p: 3, mb: 3 }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Pending Fees Summary
                </Typography>
                <Divider sx={{ mb: 2 }} />
                {Array.isArray(pendingFees) && pendingFees.length > 0 ? (
                  <Stack spacing={1}>
                    {pendingFees.map((fee) => (
                      <Stack
                        key={fee.id}
                        direction="row"
                        justifyContent="space-between"
                        alignItems="center"
                      >
                        <Stack direction="row" spacing={1} alignItems="center">
                          <Typography variant="body1">
                            {fee.feeStructureName || fee.name}
                          </Typography>
                          <Chip label={fee.status || 'Pending'} size="small" color="warning" variant="outlined" />
                        </Stack>
                        <Typography variant="body1" fontWeight={600}>
                          ${Number(fee.amount || 0).toFixed(2)}
                        </Typography>
                      </Stack>
                    ))}
                  </Stack>
                ) : (
                  <Typography variant="body2" color="text.secondary">
                    No pending fees for this student.
                  </Typography>
                )}
              </Paper>
            )}

            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button variant="outlined" onClick={() => navigate('/fees')}>
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                startIcon={<SaveIcon />}
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Collecting...' : 'Collect Fee'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
