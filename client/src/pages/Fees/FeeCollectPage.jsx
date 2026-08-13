import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
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
  installmentId: Yup.string(),
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
  receivedBy: Yup.string().trim().required('Received by is required'),
});

export default function FeeCollectPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { user } = useSelector((state) => state.auth);

  const [students, setStudents] = useState([]);
  const [studentsLoading, setStudentsLoading] = useState(false);
  const [feeStructures, setFeeStructures] = useState([]);
  const [feesLoading, setFeesLoading] = useState(false);
  const [pendingFees, setPendingFees] = useState(null);
  const [pendingLoading, setPendingLoading] = useState(false);

  const currentUserName = [user?.firstName, user?.lastName]
    .filter(Boolean)
    .join(' ')
    .trim();

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
          params: { pageSize: 1000 },
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
    const payload = { ...values };
    if (!payload.installmentId) delete payload.installmentId;
    const result = await dispatch(collectFee(payload));
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
          installmentId: '',
          amount: '',
          paymentMethod: 'Cash',
          transactionReference: '',
          remarks: '',
          paymentDate: new Date().toISOString().split('T')[0],
          receivedBy: currentUserName,
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
        }) => {
          const selectedStudent = students.find((s) => s.id === values.studentId);
          const availableFeeStructures = selectedStudent
            ? feeStructures.filter(
                (fs) => fs.classRoomId === selectedStudent.classRoomId
              )
            : feeStructures;

          const structureInstallments = (pendingFees?.installments || []).filter(
            (i) =>
              i.feeStructureId === values.feeStructureId &&
              Number(i.pendingAmount || 0) > 0
          );

          const handleFeeStructureChange = (value) => {
            setFieldValue('feeStructureId', value);
            const installments = (pendingFees?.installments || []).filter(
              (i) =>
                i.feeStructureId === value &&
                Number(i.pendingAmount || 0) > 0
            );
            const firstUnpaid = installments[0];
            if (firstUnpaid) {
              setFieldValue('installmentId', firstUnpaid.installmentId);
              setFieldValue('amount', Number(firstUnpaid.pendingAmount || 0));
            } else {
              setFieldValue('installmentId', '');
              setFieldValue('amount', '');
            }
          };

          return (
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
                      setFieldValue('feeStructureId', '');
                      setFieldValue('installmentId', '');
                      setFieldValue('amount', '');
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
                    onChange={(e) => handleFeeStructureChange(e.target.value)}
                    onBlur={handleBlur}
                    error={touched.feeStructureId && Boolean(errors.feeStructureId)}
                    helperText={
                      touched.feeStructureId && errors.feeStructureId
                        ? errors.feeStructureId
                        : selectedStudent && availableFeeStructures.length === 0
                          ? `No fee structures for ${selectedStudent.className || 'this class'}.`
                          : undefined
                    }
                    disabled={feesLoading}
                  >
                    {availableFeeStructures.map((fs) => (
                      <MenuItem key={fs.id} value={fs.id}>
                        {fs.name} - {fs.className} (${Number(fs.totalAmount || 0).toFixed(2)})
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    select
                    name="installmentId"
                    label="Installment"
                    value={values.installmentId}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    disabled={!values.feeStructureId || structureInstallments.length === 0}
                    helperText={
                      values.feeStructureId && structureInstallments.length === 0
                        ? 'No outstanding installments for this fee structure.'
                        : 'Leave blank to pay the oldest outstanding installment.'
                    }
                  >
                    {structureInstallments.map((inst) => (
                      <MenuItem
                        key={inst.installmentId}
                        value={inst.installmentId}
                        onClick={() =>
                          setFieldValue('amount', Number(inst.pendingAmount || 0))
                        }
                      >
                        {inst.name} - Pending ${Number(inst.pendingAmount || 0).toFixed(2)}
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
                    name="receivedBy"
                    label="Received By"
                    value={values.receivedBy}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.receivedBy && Boolean(errors.receivedBy)}
                    helperText={touched.receivedBy && errors.receivedBy}
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

            {!pendingLoading && pendingFees?.installments?.length > 0 && (
              <Paper sx={{ p: 3, mb: 3 }}>
                <Typography variant="h6" fontWeight={600} gutterBottom>
                  Pending Fees Summary
                </Typography>
                <Divider sx={{ mb: 2 }} />
                <Stack spacing={1}>
                  {pendingFees.installments.map((fee) => (
                    <Stack
                      key={fee.installmentId}
                      direction="row"
                      justifyContent="space-between"
                      alignItems="center"
                    >
                      <Stack direction="row" spacing={1} alignItems="center">
                        <Typography variant="body1">
                          {fee.name}
                        </Typography>
                        <Chip label={fee.status || 'Pending'} size="small" color="warning" variant="outlined" />
                      </Stack>
                      <Typography variant="body1" fontWeight={600}>
                        ${Number(fee.pendingAmount || 0).toFixed(2)}
                      </Typography>
                    </Stack>
                  ))}
                </Stack>
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
          );
        }}
      </Formik>
    </Box>
  );
}
