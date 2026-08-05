import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  Grid,
  Divider,
  FormControlLabel,
  Switch,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import SaveIcon from '@mui/icons-material/Save';
import ReceiptIcon from '@mui/icons-material/Receipt';
import PaymentsIcon from '@mui/icons-material/Payments';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import axiosInstance from '../../services/axiosInstance';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

const FEE_TYPE_OPTIONS = [
  'Tuition',
  'Admission',
  'Examination',
  'Library',
  'Laboratory',
  'Sports',
  'Transport',
  'Hostel',
  'Miscellaneous',
];

const feeStructureSchema = Yup.object({
  name: Yup.string().trim().required('Name is required'),
  classRoomId: Yup.string().required('Class is required'),
  academicYearId: Yup.string().required('Academic year is required'),
  totalAmount: Yup.number()
    .transform((value, originalValue) =>
      originalValue === '' ? undefined : value
    )
    .required('Amount is required')
    .min(0, 'Must be positive'),
  feeType: Yup.string().required('Fee type is required'),
});

const INITIAL_VALUES = {
  name: '',
  classRoomId: '',
  academicYearId: '',
  totalAmount: '',
  feeType: '',
  description: '',
  isActive: true,
  isInstallmentApplicable: false,
  numberOfInstallments: '',
};

export default function FeeStructureListPage() {
  const navigate = useNavigate();
  const [feeStructures, setFeeStructures] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingFee, setEditingFee] = useState(null);
  const [classes, setClasses] = useState([]);
  const [academicYears, setAcademicYears] = useState([]);

  useEffect(() => {
    const loadDropdowns = async () => {
      try {
        const schoolRes = await axiosInstance.get('/schools');
        const schools = schoolRes.data.data?.items || schoolRes.data.data || [];
        if (schools.length > 0) {
          const schoolId = schools[0].id;
          const [classRes, yearRes] = await Promise.all([
            axiosInstance.get(`/schools/${schoolId}/classes`),
            axiosInstance.get(`/schools/${schoolId}/academic-years`),
          ]);
          setClasses(classRes.data.data || []);
          setAcademicYears(yearRes.data.data || []);
        }
      } catch {
        setClasses([]);
        setAcademicYears([]);
      }
    };
    loadDropdowns();
  }, []);

  const fetchFeeStructures = async () => {
    setLoading(true);
    try {
      const response = await axiosInstance.get('/fees', {
        params: { pageNumber: page + 1, pageSize: rowsPerPage },
      });
      setFeeStructures(response.data.data);
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to fetch fee structures');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchFeeStructures();
  }, [page, rowsPerPage]);

  const columns = [
    { id: 'name', header: 'Name', accessor: 'name', minWidth: 150 },
    { id: 'className', header: 'Class', accessor: 'className', minWidth: 100 },
    {
      id: 'totalAmount',
      header: 'Amount',
      accessor: 'totalAmount',
      minWidth: 100,
      render: (value) => `$${Number(value || 0).toFixed(2)}`,
    },
    { id: 'feeType', header: 'Fee Type', accessor: 'feeType', minWidth: 120 },
    {
      id: 'isActive',
      header: 'Status',
      accessor: 'isActive',
      minWidth: 100,
      render: (value) => (
        <Chip
          label={value ? 'Active' : 'Inactive'}
          color={value ? 'success' : 'default'}
          size="small"
          variant="outlined"
        />
      ),
    },
  ];

  const handlePageChange = (_, newPage) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleEdit = (row) => {
    setEditingFee(row);
    setDialogOpen(true);
  };

  const handleDelete = (row) => {
    setDeleteTarget(row);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleteLoading(true);
    try {
      await axiosInstance.delete(`/fees/${deleteTarget.id}`);
      toast.success('Fee structure deleted successfully');
      setDeleteTarget(null);
      fetchFeeStructures();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to delete fee structure');
    } finally {
      setDeleteLoading(false);
    }
  };

  const handleDialogClose = () => {
    setDialogOpen(false);
    setEditingFee(null);
  };

  const handleDialogSubmit = async (values, { setSubmitting }) => {
    try {
      const payload = {
        name: values.name,
        description: values.description,
        classRoomId: values.classRoomId,
        academicYearId: values.academicYearId,
        totalAmount: Number(values.totalAmount),
        feeType: values.feeType,
        isActive: values.isActive,
        isInstallmentApplicable: values.isInstallmentApplicable,
        numberOfInstallments: values.isInstallmentApplicable && values.numberOfInstallments
          ? Number(values.numberOfInstallments) : null,
      };
      if (editingFee) {
        await axiosInstance.put(`/fees/${editingFee.id}`, payload);
        toast.success('Fee structure updated successfully');
      } else {
        await axiosInstance.post('/fees', payload);
        toast.success('Fee structure created successfully');
      }
      handleDialogClose();
      fetchFeeStructures();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Failed to save fee structure');
    } finally {
      setSubmitting(false);
    }
  };

  const getEditInitialValues = () => {
    if (!editingFee) return INITIAL_VALUES;
    return {
      name: editingFee.name || '',
      classRoomId: editingFee.classRoomId || '',
      academicYearId: editingFee.academicYearId || '',
      totalAmount: editingFee.totalAmount ?? '',
      feeType: editingFee.feeType || '',
      description: editingFee.description || '',
      isActive: editingFee.isActive ?? true,
      isInstallmentApplicable: editingFee.isInstallmentApplicable ?? false,
      numberOfInstallments: editingFee.numberOfInstallments ?? '',
    };
  };

  return (
    <Box>
      <PageHeader
        title="Fee Structures"
        subtitle={`Total ${feeStructures.totalCount || 0} fee structures`}
        actions={
          <>
            <Button
              variant="outlined"
              startIcon={<ReceiptIcon />}
              onClick={() => navigate('/fees/receipts')}
            >
              Receipts
            </Button>
            <Button
              variant="outlined"
              startIcon={<PaymentsIcon />}
              onClick={() => navigate('/fees/collect')}
            >
              Collect Fee
            </Button>
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => {
                setEditingFee(null);
                setDialogOpen(true);
              }}
            >
              Add Fee Structure
            </Button>
          </>
        }
      />

      <DataTable
        columns={columns}
        rows={feeStructures.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={feeStructures.totalCount || 0}
        searchPlaceholder="Search fee structures..."
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onEdit={handleEdit}
        onDelete={handleDelete}
        emptyMessage="No fee structures found"
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Fee Structure"
        message={`Are you sure you want to delete "${deleteTarget?.name}"? This action cannot be undone.`}
        confirmText="Delete"
        loading={deleteLoading}
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />

      <Dialog
        open={dialogOpen}
        onClose={handleDialogClose}
        maxWidth="sm"
        fullWidth
        PaperProps={{ sx: { borderRadius: 2 } }}
      >
        <DialogTitle sx={{ fontWeight: 600 }}>
          {editingFee ? 'Edit Fee Structure' : 'Add Fee Structure'}
        </DialogTitle>
        <Divider />
        <Formik
          initialValues={getEditInitialValues()}
          validationSchema={feeStructureSchema}
          onSubmit={handleDialogSubmit}
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
              <DialogContent>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12 }}>
                    <TextField
                      fullWidth
                      name="name"
                      label="Fee Name"
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
                      name="classRoomId"
                      label="Class"
                      value={values.classRoomId}
                      onChange={handleChange}
                      onBlur={handleBlur}
                      error={touched.classRoomId && Boolean(errors.classRoomId)}
                      helperText={touched.classRoomId && errors.classRoomId}
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
                      name="academicYearId"
                      label="Academic Year"
                      value={values.academicYearId}
                      onChange={handleChange}
                      onBlur={handleBlur}
                      error={touched.academicYearId && Boolean(errors.academicYearId)}
                      helperText={touched.academicYearId && errors.academicYearId}
                    >
                      {academicYears.map((year) => (
                        <MenuItem key={year.id} value={year.id}>
                          {year.name}
                        </MenuItem>
                      ))}
                    </TextField>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      fullWidth
                      select
                      name="feeType"
                      label="Fee Type"
                      value={values.feeType}
                      onChange={handleChange}
                      onBlur={handleBlur}
                      error={touched.feeType && Boolean(errors.feeType)}
                      helperText={touched.feeType && errors.feeType}
                    >
                      {FEE_TYPE_OPTIONS.map((option) => (
                        <MenuItem key={option} value={option}>
                          {option}
                        </MenuItem>
                      ))}
                    </TextField>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      fullWidth
                      name="totalAmount"
                      label="Amount"
                      type="number"
                      value={values.totalAmount}
                      onChange={handleChange}
                      onBlur={handleBlur}
                      error={touched.totalAmount && Boolean(errors.totalAmount)}
                      helperText={touched.totalAmount && errors.totalAmount}
                    />
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
                    <FormControlLabel
                      control={
                        <Switch
                          name="isActive"
                          checked={values.isActive}
                          onChange={handleChange}
                          color="primary"
                        />
                      }
                      label="Active"
                    />
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <FormControlLabel
                      control={
                        <Switch
                          name="isInstallmentApplicable"
                          checked={values.isInstallmentApplicable}
                          onChange={handleChange}
                          color="primary"
                        />
                      }
                      label="Installments"
                    />
                  </Grid>
                  {values.isInstallmentApplicable && (
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField
                        fullWidth
                        name="numberOfInstallments"
                        label="Number of Installments"
                        type="number"
                        value={values.numberOfInstallments}
                        onChange={handleChange}
                        onBlur={handleBlur}
                      />
                    </Grid>
                  )}
                </Grid>
              </DialogContent>
              <DialogActions sx={{ px: 3, pb: 2.5 }}>
                <Button onClick={handleDialogClose} variant="outlined" disabled={isSubmitting}>
                  Cancel
                </Button>
                <Button
                  type="submit"
                  variant="contained"
                  startIcon={<SaveIcon />}
                  disabled={isSubmitting}
                >
                  {isSubmitting ? 'Saving...' : editingFee ? 'Update' : 'Create'}
                </Button>
              </DialogActions>
            </Form>
          )}
        </Formik>
      </Dialog>
    </Box>
  );
}
