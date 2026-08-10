import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Formik, Form } from 'formik';
import {
  Box,
  Tabs,
  Tab,
  Button,
  Grid,
  TextField,
  MenuItem,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stack,
  Chip,
  Paper,
  Divider,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import * as Yup from 'yup';
import {
  fetchIncome,
  createIncome,
  updateIncome,
  deleteIncome,
  fetchExpense,
  createExpense,
  updateExpense,
  deleteExpense,
  fetchLedger,
} from '../../store/slices/accountSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import axiosInstance from '../../services/axiosInstance';
import toast from 'react-hot-toast';

const INCOME_CATEGORIES = ['Tuition Fee', 'Donation', 'Government Fund', 'Service Income', 'Other'];
const EXPENSE_CATEGORIES = ['Salary', 'Utilities', 'Maintenance', 'Purchase', 'Transport', 'Other'];
const PAYMENT_METHODS = ['Cash', 'Bank Transfer', 'Cheque', 'Online'];

const incomeSchema = Yup.object({
  title: Yup.string().trim().required('Title is required'),
  description: Yup.string().trim().required('Description is required'),
  amount: Yup.number()
    .transform((value, originalValue) => (originalValue === '' ? undefined : value))
    .positive('Amount must be positive')
    .required('Amount is required'),
  category: Yup.string().required('Category is required'),
  date: Yup.date().nullable().required('Date is required'),
  referenceNumber: Yup.string().trim(),
  paymentMethod: Yup.string().required('Payment method is required'),
});

const expenseSchema = Yup.object({
  title: Yup.string().trim().required('Title is required'),
  description: Yup.string().trim().required('Description is required'),
  amount: Yup.number()
    .transform((value, originalValue) => (originalValue === '' ? undefined : value))
    .positive('Amount must be positive')
    .required('Amount is required'),
  category: Yup.string().required('Category is required'),
  date: Yup.date().nullable().required('Date is required'),
  referenceNumber: Yup.string().trim(),
  paymentMethod: Yup.string().required('Payment method is required'),
  vendor: Yup.string().trim(),
});

const toISODate = (d) => {
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
};

const defaultLedgerFromDate = () => {
  const d = new Date();
  d.setDate(d.getDate() - 30);
  return toISODate(d);
};

const defaultLedgerToDate = () => toISODate(new Date());

const formatCurrency = (value) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value || 0);

export default function AccountsPage() {
  const dispatch = useDispatch();
  const { income, expense, ledger, loading } = useSelector((state) => state.accounts);

  const [activeTab, setActiveTab] = useState(0);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingTarget, setEditingTarget] = useState(null);
  const [viewTarget, setViewTarget] = useState(null);
  const [fromDate, setFromDate] = useState(defaultLedgerFromDate);
  const [toDate, setToDate] = useState(defaultLedgerToDate);

  useEffect(() => {
    if (activeTab === 0) {
      dispatch(fetchIncome({ page: page + 1, pageSize: rowsPerPage }));
    } else if (activeTab === 1) {
      dispatch(fetchExpense({ page: page + 1, pageSize: rowsPerPage }));
    }
  }, [dispatch, activeTab, page, rowsPerPage]);

  useEffect(() => {
    if (activeTab === 2 && fromDate && toDate) {
      dispatch(fetchLedger({ fromDate, toDate }));
    }
  }, [dispatch, activeTab, fromDate, toDate]);

  const handleTabChange = (_, newValue) => {
    setActiveTab(newValue);
    setPage(0);
    setDialogOpen(false);
    setEditingTarget(null);
    setViewTarget(null);
  };

  const handlePageChange = (_, newPage) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleDelete = (row) => {
    setDeleteTarget(row);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const action = activeTab === 0 ? deleteIncome : deleteExpense;
    const result = await dispatch(action(deleteTarget.id));
    if (action.fulfilled.match(result)) {
      toast.success(`${activeTab === 0 ? 'Income' : 'Expense'} deleted successfully`);
      setDeleteTarget(null);
      const fetchAction = activeTab === 0 ? fetchIncome : fetchExpense;
      dispatch(fetchAction({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed to delete');
    }
  };

  const handleView = (row) => {
    setViewTarget(row);
  };

  const handleEdit = (row) => {
    setEditingTarget(row);
    setDialogOpen(true);
  };

  const incomeColumns = [
    { id: 'date', header: 'Date', accessor: 'date', minWidth: 110, render: (v) => (v ? new Date(v).toLocaleDateString() : '') },
    { id: 'title', header: 'Title', accessor: 'title', minWidth: 150 },
    { id: 'description', header: 'Description', accessor: 'description', minWidth: 180 },
    { id: 'category', header: 'Category', accessor: 'category', minWidth: 140 },
    { id: 'amount', header: 'Amount', accessor: 'amount', minWidth: 120, render: (v) => formatCurrency(v) },
    { id: 'referenceNumber', header: 'Reference', accessor: 'referenceNumber', minWidth: 120 },
    { id: 'paymentMethod', header: 'Payment Method', accessor: 'paymentMethod', minWidth: 130 },
  ];

  const expenseColumns = [
    { id: 'date', header: 'Date', accessor: 'date', minWidth: 110, render: (v) => (v ? new Date(v).toLocaleDateString() : '') },
    { id: 'title', header: 'Title', accessor: 'title', minWidth: 150 },
    { id: 'description', header: 'Description', accessor: 'description', minWidth: 180 },
    { id: 'category', header: 'Category', accessor: 'category', minWidth: 140 },
    { id: 'amount', header: 'Amount', accessor: 'amount', minWidth: 120, render: (v) => formatCurrency(v) },
    { id: 'vendor', header: 'Vendor', accessor: 'vendor', minWidth: 120 },
    { id: 'referenceNumber', header: 'Reference', accessor: 'referenceNumber', minWidth: 120 },
    { id: 'paymentMethod', header: 'Payment Method', accessor: 'paymentMethod', minWidth: 130 },
  ];

  const ledgerColumns = [
    { id: 'date', header: 'Date', accessor: 'date', minWidth: 110, render: (v) => (v ? new Date(v).toLocaleDateString() : '') },
    {
      id: 'type',
      header: 'Type',
      accessor: 'type',
      minWidth: 100,
      render: (v) => (
        <Chip label={v} color={v === 'Income' ? 'success' : 'error'} size="small" variant="outlined" />
      ),
    },
    { id: 'title', header: 'Description', accessor: 'title', minWidth: 200 },
    { id: 'referenceNumber', header: 'Reference', accessor: 'referenceNumber', minWidth: 120 },
    { id: 'debit', header: 'Debit', accessor: 'debit', minWidth: 120, render: (v) => (v ? formatCurrency(v) : '-') },
    { id: 'credit', header: 'Credit', accessor: 'credit', minWidth: 120, render: (v) => (v ? formatCurrency(v) : '-') },
    { id: 'balance', header: 'Balance', accessor: 'balance', minWidth: 120, render: (v) => formatCurrency(v) },
  ];

  const summarySubtitle = `Income: ${formatCurrency((income.items || []).reduce((sum, i) => sum + (i.amount || 0), 0))} | Expenses: ${formatCurrency((expense.items || []).reduce((sum, e) => sum + (e.amount || 0), 0))} | Balance: ${formatCurrency((income.items || []).reduce((sum, i) => sum + (i.amount || 0), 0) - (expense.items || []).reduce((sum, e) => sum + (e.amount || 0), 0))}`;

  return (
    <Box>
      <PageHeader
        title="Accounts"
        subtitle={summarySubtitle}
        actions={
          activeTab !== 2 ? (
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => {
                setEditingTarget(null);
                setDialogOpen(true);
              }}
            >
              {activeTab === 0 ? 'Add Income' : 'Add Expense'}
            </Button>
          ) : null
        }
      />

      <Paper sx={{ mb: 3 }}>
        <Tabs value={activeTab} onChange={handleTabChange} sx={{ px: 2 }}>
          <Tab label="Income" />
          <Tab label="Expenses" />
          <Tab label="Ledger" />
        </Tabs>
        <Divider />
      </Paper>

      {activeTab === 0 && (
        <DataTable
          columns={incomeColumns}
          rows={income.items || []}
          loading={loading}
          page={page}
          rowsPerPage={rowsPerPage}
          totalCount={income.totalCount || 0}
          searchPlaceholder="Search income..."
          onPageChange={handlePageChange}
          onRowsPerPageChange={handleRowsPerPageChange}
          onView={handleView}
          onEdit={handleEdit}
          onDelete={handleDelete}
          emptyMessage="No income records found"
        />
      )}

      {activeTab === 1 && (
        <DataTable
          columns={expenseColumns}
          rows={expense.items || []}
          loading={loading}
          page={page}
          rowsPerPage={rowsPerPage}
          totalCount={expense.totalCount || 0}
          searchPlaceholder="Search expenses..."
          onPageChange={handlePageChange}
          onRowsPerPageChange={handleRowsPerPageChange}
          onView={handleView}
          onEdit={handleEdit}
          onDelete={handleDelete}
          emptyMessage="No expense records found"
        />
      )}

      {activeTab === 2 && (
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" fontWeight={600} gutterBottom>
            Date Range
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Stack direction="row" spacing={2}>
            <TextField
              label="From Date"
              type="date"
              size="small"
              value={fromDate}
              onChange={(e) => setFromDate(e.target.value)}
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              label="To Date"
              type="date"
              size="small"
              value={toDate}
              onChange={(e) => setToDate(e.target.value)}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Stack>
        </Paper>
      )}

      {activeTab === 2 && (
        <DataTable
          columns={ledgerColumns}
          rows={ledger || []}
          loading={loading}
          page={0}
          rowsPerPage={100}
          totalCount={(ledger || []).length}
          searchPlaceholder="Search ledger..."
          enableSearch
          showActions={false}
          emptyMessage="Select a date range to view ledger"
        />
      )}

      {activeTab !== 2 && dialogOpen && (
        <AccountFormDialog
          open={dialogOpen}
          onClose={() => {
            setDialogOpen(false);
            setEditingTarget(null);
          }}
          type={activeTab === 0 ? 'income' : 'expense'}
          editingRecord={editingTarget}
        />
      )}

      {viewTarget && (
        <AccountViewDialog
          open={!!viewTarget}
          onClose={() => setViewTarget(null)}
          record={viewTarget}
          type={activeTab === 0 ? 'income' : 'expense'}
        />
      )}

      <ConfirmDialog
        open={!!deleteTarget}
        title={`Delete ${activeTab === 0 ? 'Income' : 'Expense'}`}
        message={`Are you sure you want to delete this ${activeTab === 0 ? 'income' : 'expense'} record? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  );
}

function AccountFormDialog({ open, onClose, type, editingRecord }) {
  const dispatch = useDispatch();
  const categories = type === 'income' ? INCOME_CATEGORIES : EXPENSE_CATEGORIES;
  const isEditing = Boolean(editingRecord);

  const buildInitialValues = () => {
    if (!editingRecord) {
      return {
        title: '',
        description: '',
        amount: '',
        category: '',
        date: '',
        referenceNumber: '',
        paymentMethod: '',
        vendor: '',
      };
    }
    return {
      title: editingRecord.title || '',
      description: editingRecord.description || '',
      amount: editingRecord.amount ?? '',
      category: editingRecord.category || '',
      date: editingRecord.date ? toISODate(new Date(editingRecord.date)) : '',
      referenceNumber: editingRecord.referenceNumber || '',
      paymentMethod: editingRecord.paymentMethod || '',
      vendor: editingRecord.vendor || '',
    };
  };

  const initialValues = buildInitialValues();

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      let schoolId = '';
      try {
        const res = await axiosInstance.get('/schools');
        const items = res.data.data?.items || [];
        if (items.length > 0) schoolId = items[0].id;
      } catch {}

      const payload = { ...values, ...(schoolId ? { schoolId } : {}) };

      if (isEditing) {
        const action = type === 'income' ? updateIncome : updateExpense;
        const result = await dispatch(action({ id: editingRecord.id, data: payload }));
        if (action.fulfilled.match(result)) {
          toast.success(`${type === 'income' ? 'Income' : 'Expense'} updated successfully`);
          onClose();
        } else {
          toast.error(result.payload || 'Failed to update');
        }
      } else {
        const action = type === 'income' ? createIncome : createExpense;
        const result = await dispatch(action(payload));
        if (action.fulfilled.match(result)) {
          toast.success(`${type === 'income' ? 'Income' : 'Expense'} created successfully`);
          onClose();
        } else {
          toast.error(result.payload || 'Failed to create');
        }
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth PaperProps={{ sx: { borderRadius: 2 } }}>
      <DialogTitle sx={{ fontWeight: 600 }}>
        {type === 'income'
          ? isEditing
            ? 'Edit Income'
            : 'Add Income'
          : isEditing
            ? 'Edit Expense'
            : 'Add Expense'}
      </DialogTitle>
      <Formik
        initialValues={initialValues}
        validationSchema={type === 'income' ? incomeSchema : expenseSchema}
        onSubmit={handleSubmit}
      >
        {({ values, errors, touched, handleChange, handleBlur, isSubmitting }) => (
          <Form>
            <DialogContent dividers>
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="title"
                    label="Title"
                    value={values.title}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.title && Boolean(errors.title)}
                    helperText={touched.title && errors.title}
                  />
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
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth
                    name="description"
                    label="Description"
                    value={values.description}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.description && Boolean(errors.description)}
                    helperText={touched.description && errors.description}
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
                    {categories.map((option) => (
                      <MenuItem key={option} value={option}>
                        {option}
                      </MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField
                    fullWidth
                    name="date"
                    label="Date"
                    type="date"
                    value={values.date}
                    onChange={handleChange}
                    onBlur={handleBlur}
                    error={touched.date && Boolean(errors.date)}
                    helperText={touched.date && errors.date}
                    slotProps={{ inputLabel: { shrink: true } }}
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
                    name="referenceNumber"
                    label="Reference Number"
                    value={values.referenceNumber}
                    onChange={handleChange}
                    onBlur={handleBlur}
                  />
                </Grid>
                {type === 'expense' && (
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <TextField
                      fullWidth
                      name="vendor"
                      label="Vendor"
                      value={values.vendor}
                      onChange={handleChange}
                      onBlur={handleBlur}
                    />
                  </Grid>
                )}
              </Grid>
            </DialogContent>
            <DialogActions sx={{ px: 3, py: 2 }}>
              <Button onClick={onClose} variant="outlined" disabled={isSubmitting}>
                Cancel
              </Button>
              <Button type="submit" variant="contained" disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : isEditing ? 'Update' : 'Save'}
              </Button>
            </DialogActions>
          </Form>
        )}
      </Formik>
    </Dialog>
  );
}

function AccountViewDialog({ open, onClose, record, type }) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth PaperProps={{ sx: { borderRadius: 2 } }}>
      <DialogTitle sx={{ fontWeight: 600 }}>
        {type === 'income' ? 'Income' : 'Expense'} Details
      </DialogTitle>
      <Divider />
      <DialogContent>
        {record && (
          <Grid container spacing={2} sx={{ mt: 0 }}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Typography variant="subtitle2" color="text.secondary">
                Title
              </Typography>
              <Typography variant="body1" fontWeight={600}>
                {record.title || 'N/A'}
              </Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Typography variant="subtitle2" color="text.secondary">
                Amount
              </Typography>
              <Typography variant="body1" fontWeight={600} color="primary.main">
                {formatCurrency(record.amount)}
              </Typography>
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Typography variant="subtitle2" color="text.secondary">
                Description
              </Typography>
              <Typography variant="body1">{record.description || 'N/A'}</Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Typography variant="subtitle2" color="text.secondary">
                Category
              </Typography>
              <Typography variant="body1">{record.category || 'N/A'}</Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Typography variant="subtitle2" color="text.secondary">
                Date
              </Typography>
              <Typography variant="body1">
                {record.date ? new Date(record.date).toLocaleDateString() : 'N/A'}
              </Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Typography variant="subtitle2" color="text.secondary">
                Payment Method
              </Typography>
              <Typography variant="body1">{record.paymentMethod || 'N/A'}</Typography>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <Typography variant="subtitle2" color="text.secondary">
                Reference Number
              </Typography>
              <Typography variant="body1">{record.referenceNumber || 'N/A'}</Typography>
            </Grid>
            {type === 'expense' && (
              <Grid size={{ xs: 12, sm: 6 }}>
                <Typography variant="subtitle2" color="text.secondary">
                  Vendor
                </Typography>
                <Typography variant="body1">{record.vendor || 'N/A'}</Typography>
              </Grid>
            )}
          </Grid>
        )}
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2.5 }}>
        <Button variant="outlined" onClick={onClose}>
          Close
        </Button>
      </DialogActions>
    </Dialog>
  );
}
