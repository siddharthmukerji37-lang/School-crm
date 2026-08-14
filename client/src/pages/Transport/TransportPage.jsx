import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Tabs, Tab, Button, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Typography, Stack, MenuItem, Chip, CircularProgress } from '@mui/material';
import Grid from '@mui/material/Grid2';
import AddIcon from '@mui/icons-material/Add';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
  fetchRoutes, createRoute, updateRoute, deleteRoute,
  fetchVehicles, createVehicle, updateVehicle, deleteVehicle,
  fetchAllocations, allocateTransport, deallocateTransport,
} from '../../store/slices/transportSlice';
import { fetchStudents } from '../../store/slices/studentSlice';
import { findCurrentStudent, filterStudentAllocations } from '../../utils/studentAllocationUtils';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

const routeSchema = Yup.object({
  name: Yup.string().trim().required('Name is required'),
  startPoint: Yup.string().trim().required('Start location is required'),
  endPoint: Yup.string().trim().required('End location is required'),
  distance: Yup.number().transform((v, o) => o === '' ? undefined : v).min(0),
  monthlyFee: Yup.number().transform((v, o) => o === '' ? undefined : v).min(0).required('Fare is required'),
});

const vehicleSchema = Yup.object({
  registrationNumber: Yup.string().trim().required('Registration number is required'),
  vehicleType: Yup.string().required('Vehicle type is required'),
  capacity: Yup.number().transform((v, o) => o === '' ? undefined : v).min(1).required('Capacity is required'),
  driverName: Yup.string().trim().required('Driver name is required'),
  routeId: Yup.string().required('Route is required'),
  isActive: Yup.boolean(),
});

const VEHICLE_TYPES = ['Bus', 'Van', 'Car', 'Mini Bus'];

export default function TransportPage() {
  const dispatch = useDispatch();
  const { routes, vehicles, allocations } = useSelector((state) => state.transport);
  const { students } = useSelector((state) => state.students);
  const { user } = useSelector((state) => state.auth);
  const userRole = user?.roles?.[0] || user?.role || 'Admin';
  const isAdmin = ['SuperAdmin', 'Admin'].includes(userRole);
  const isStudent = userRole === 'Student';
  const currentStudent = findCurrentStudent(students, user);

  const [tab, setTab] = useState(0);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleteType, setDeleteType] = useState('');
  const [viewItem, setViewItem] = useState(null);
  const [allocOpen, setAllocOpen] = useState(false);
  const [allocForm, setAllocForm] = useState({ studentId: '', routeId: '' });
  const [allocSubmitting, setAllocSubmitting] = useState(false);
  const [deallocTarget, setDeallocTarget] = useState(null);

  useEffect(() => {
    const params = { page: page + 1, pageSize: rowsPerPage };

    if (isStudent) {
      dispatch(fetchAllocations({ ...params, studentId: currentStudent?.id }));
      return;
    }

    if (tab === 0) dispatch(fetchRoutes(params));
    else if (tab === 1) dispatch(fetchVehicles(params));
    else dispatch(fetchAllocations(params));
  }, [dispatch, isStudent, currentStudent?.id, tab, page, rowsPerPage]);

  useEffect(() => {
    dispatch(fetchStudents({ page: 1, pageSize: 500 }));
  }, [dispatch]);

  const routeColumns = [
    { id: 'name', header: 'Route Name', accessor: 'name', minWidth: 160 },
    { id: 'startPoint', header: 'Start', accessor: 'startPoint', minWidth: 140 },
    { id: 'endPoint', header: 'End', accessor: 'endPoint', minWidth: 140 },
    { id: 'distance', header: 'Distance (km)', accessor: 'distance', minWidth: 110, align: 'center' },
    { id: 'monthlyFee', header: 'Fare', accessor: 'monthlyFee', minWidth: 100, align: 'right', render: (v) => `$${Number(v || 0).toFixed(2)}` },
    {
      id: 'isActive', header: 'Status', accessor: 'isActive', minWidth: 100,
      render: (v) => (
        <Chip
          label={v ? 'Active' : 'Inactive'}
          color={v ? 'success' : 'error'}
          size="small"
          variant="outlined"
        />
      ),
    },
  ];

  const vehicleColumns = [
    { id: 'registrationNumber', header: 'Reg. Number', accessor: 'registrationNumber', minWidth: 130 },
    { id: 'vehicleType', header: 'Type', accessor: 'vehicleType', minWidth: 100 },
    { id: 'routeName', header: 'Route', accessor: 'routeName', minWidth: 130 },
    { id: 'capacity', header: 'Capacity', accessor: 'capacity', minWidth: 90, align: 'center' },
    { id: 'driverName', header: 'Driver', accessor: 'driverName', minWidth: 150 },
    {
      id: 'status', header: 'Status', accessor: 'isActive', minWidth: 100,
      render: (v) => (
        <Chip
          label={v ? 'Active' : 'Inactive'}
          color={v ? 'success' : 'error'}
          size="small"
          variant="outlined"
        />
      ),
    },
  ];

  const allocationColumns = [
    { id: 'studentName', header: 'Student', accessor: 'studentName', minWidth: 160 },
    { id: 'routeName', header: 'Route', accessor: 'routeName', minWidth: 140 },
    { id: 'monthlyFee', header: 'Monthly Fee', accessor: 'monthlyFee', minWidth: 110, render: (v) => `$${Number(v || 0).toFixed(2)}` },
    {
      id: 'status', header: 'Status', accessor: 'isActive', minWidth: 100,
      render: (v) => (
        <Chip
          label={v ? 'Active' : 'Inactive'}
          color={v ? 'success' : 'error'}
          size="small"
          variant="outlined"
        />
      ),
    },
  ];

  const handleOpenDialog = (item = null) => { setEditItem(item); setDialogOpen(true); };
  const handleCloseDialog = () => { setEditItem(null); setDialogOpen(false); };

  const handleDelete = (item, type) => { setDeleteTarget(item); setDeleteType(type); };
  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const action = deleteType === 'route'
      ? await dispatch(deleteRoute(deleteTarget.id))
      : await dispatch(deleteVehicle(deleteTarget.id));
    if ((deleteType === 'route' ? deleteRoute : deleteVehicle).fulfilled.match(action)) {
      toast.success(`${deleteType} deleted`);
      setDeleteTarget(null);
      setDeleteType('');
      const params = { page: page + 1, pageSize: rowsPerPage };
      if (tab === 0) dispatch(fetchRoutes(params));
      else dispatch(fetchVehicles(params));
    } else {
      toast.error(action.payload || 'Failed');
    }
  };

  const handleAllocate = async () => {
    if (!allocForm.studentId || !allocForm.routeId) {
      toast.error('Select both a student and a route');
      return;
    }
    setAllocSubmitting(true);
    try {
      const result = await dispatch(allocateTransport(allocForm));
      if (allocateTransport.fulfilled.match(result)) {
        toast.success('Transport allocated');
        setAllocOpen(false);
        dispatch(fetchAllocations({ page: page + 1, pageSize: rowsPerPage }));
      } else {
        toast.error(result.payload || 'Failed to allocate');
      }
    } finally {
      setAllocSubmitting(false);
    }
  };

  const confirmDeallocate = async () => {
    if (!deallocTarget) return;
    const result = await dispatch(deallocateTransport(deallocTarget.id));
    if (deallocateTransport.fulfilled.match(result)) {
      toast.success('Transport deallocated');
      setDeallocTarget(null);
      dispatch(fetchAllocations({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed');
    }
  };

  const studentScopedAllocations = isStudent
    ? filterStudentAllocations(allocations?.items || [], currentStudent)
    : allocations?.items || [];
  const currentData = isStudent
    ? { items: studentScopedAllocations, totalCount: studentScopedAllocations.length }
    : tab === 0 ? routes : tab === 1 ? vehicles : allocations;
  const currentColumns = isStudent ? allocationColumns : tab === 0 ? routeColumns : tab === 1 ? vehicleColumns : allocationColumns;

  const initialValues = tab === 0
    ? {
        name: editItem?.name || '',
        startPoint: editItem?.startPoint || '',
        endPoint: editItem?.endPoint || '',
        distance: editItem?.distance ?? '',
        monthlyFee: editItem?.monthlyFee ?? '',
      }
    : {
        registrationNumber: editItem?.registrationNumber || '',
        vehicleType: editItem?.vehicleType || '',
        capacity: editItem?.capacity ?? '',
        driverName: editItem?.driverName || '',
        routeId: editItem?.routeId || '',
        isActive: editItem?.isActive ?? true,
      };

  return (
    <Box>
      <PageHeader title="Transport Management" subtitle={isStudent ? 'Your assigned route' : 'Manage routes, vehicles and student allocations'} />
      <Tabs value={isStudent ? 2 : tab} onChange={(_, v) => { setTab(v); setPage(0); }} sx={{ mb: 2 }}>
        {!isStudent && <Tab label="Routes" />}
        {!isStudent && <Tab label="Vehicles" />}
        <Tab label={isStudent ? 'My Allocation' : 'Allocations'} />
      </Tabs>

      {!isStudent && isAdmin && (
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => (tab === 2 ? (() => { setAllocForm({ studentId: '', routeId: '' }); setAllocOpen(true); })() : handleOpenDialog())}
          >
            {tab === 0 ? 'Add Route' : tab === 1 ? 'Add Vehicle' : 'Allocate Transport'}
          </Button>
        </Box>
      )}

      <DataTable
        columns={currentColumns}
        rows={currentData?.items || []}
        loading={false}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={currentData?.totalCount || 0}
        onPageChange={(_, p) => setPage(p)}
        onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
        onView={(row) => setViewItem(row)}
        onEdit={isAdmin && !isStudent && tab < 2 ? (row) => handleOpenDialog(row) : undefined}
        onDelete={isAdmin && !isStudent && tab < 2 ? (row) => handleDelete(row, tab === 0 ? 'route' : 'vehicle') : undefined}
        onReturn={isAdmin && !isStudent && tab === 2 ? (row) => setDeallocTarget(row) : undefined}
        emptyMessage={isStudent ? 'No transport allocation found' : `No ${tab === 0 ? 'routes' : tab === 1 ? 'vehicles' : 'allocations'} found`}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title={`Delete ${deleteType}`}
        message={`Are you sure you want to delete this ${deleteType}?`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => { setDeleteTarget(null); setDeleteType(''); }}
      />

      <ConfirmDialog
        open={!!deallocTarget}
        title="Deallocate Transport"
        message={`Remove "${deallocTarget?.studentName}" from the "${deallocTarget?.routeName}" route?`}
        confirmText="Deallocate"
        onConfirm={confirmDeallocate}
        onCancel={() => setDeallocTarget(null)}
      />

      <Dialog open={!!viewItem} onClose={() => setViewItem(null)} maxWidth="sm" fullWidth>
        <DialogTitle>{tab === 0 ? 'Route Details' : tab === 1 ? 'Vehicle Details' : 'Allocation Details'}</DialogTitle>
        <DialogContent dividers>
          {tab === 0 && viewItem && (
            <Stack spacing={1.5}>
              <Typography><b>Name:</b> {viewItem.name}</Typography>
              <Typography><b>Start:</b> {viewItem.startPoint}</Typography>
              <Typography><b>End:</b> {viewItem.endPoint}</Typography>
              <Typography><b>Distance:</b> {viewItem.distance} km</Typography>
              <Typography><b>Fare:</b> ${Number(viewItem.monthlyFee || 0).toFixed(2)}</Typography>
              <Typography><b>Status:</b> {viewItem.isActive ? 'Active' : 'Inactive'}</Typography>
            </Stack>
          )}
          {tab === 1 && viewItem && (
            <Stack spacing={1.5}>
              <Typography><b>Reg. Number:</b> {viewItem.registrationNumber}</Typography>
              <Typography><b>Type:</b> {viewItem.vehicleType}</Typography>
              <Typography><b>Route:</b> {viewItem.routeName}</Typography>
              <Typography><b>Capacity:</b> {viewItem.capacity}</Typography>
              <Typography><b>Driver:</b> {viewItem.driverName}{viewItem.driverPhone ? ` (${viewItem.driverPhone})` : ''}</Typography>
              <Typography><b>Status:</b> {viewItem.isActive ? 'Active' : 'Inactive'}</Typography>
            </Stack>
          )}
          {tab === 2 && viewItem && (
            <Stack spacing={1.5}>
              <Typography><b>Student:</b> {viewItem.studentName}</Typography>
              <Typography><b>Route:</b> {viewItem.routeName}</Typography>
              <Typography><b>Monthly Fee:</b> ${Number(viewItem.monthlyFee || 0).toFixed(2)}</Typography>
              <Typography><b>Status:</b> {viewItem.isActive ? 'Active' : 'Inactive'}</Typography>
            </Stack>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewItem(null)} variant="outlined">Close</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={allocOpen} onClose={() => setAllocOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Allocate Transport</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              select
              fullWidth
              label="Select Student"
              value={allocForm.studentId}
              onChange={(e) => setAllocForm({ ...allocForm, studentId: e.target.value })}
            >
              {(students?.items || []).map((s) => (
                <MenuItem key={s.id} value={s.id}>
                  {s.firstName} {s.lastName}{s.admissionNumber ? ` (${s.admissionNumber})` : ''}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              select
              fullWidth
              label="Select Route"
              value={allocForm.routeId}
              onChange={(e) => setAllocForm({ ...allocForm, routeId: e.target.value })}
            >
              {(routes.items || [])
                .filter((r) => r.isActive)
                .map((r) => (
                  <MenuItem key={r.id} value={r.id}>
                    {r.name} ({r.startPoint} → {r.endPoint})
                  </MenuItem>
                ))}
            </TextField>
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setAllocOpen(false)} variant="outlined">Cancel</Button>
          <Button onClick={handleAllocate} variant="contained" disabled={allocSubmitting}>
            {allocSubmitting ? <CircularProgress size={20} /> : 'Allocate'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editItem ? `Edit ${tab === 0 ? 'Route' : 'Vehicle'}` : `Add ${tab === 0 ? 'Route' : 'Vehicle'}`}</DialogTitle>
        <Formik
          key={tab === 0 ? `route-${editItem?.id || 'new'}` : `vehicle-${editItem?.id || 'new'}`}
          initialValues={initialValues}
          validationSchema={tab === 0 ? routeSchema : vehicleSchema}
          onSubmit={async (values, { setSubmitting }) => {
            try {
              const payload = { ...values };
              if (payload.distance !== undefined && payload.distance !== '') payload.distance = Number(payload.distance);
              if (payload.monthlyFee !== undefined && payload.monthlyFee !== '') payload.monthlyFee = Number(payload.monthlyFee);
              if (payload.capacity !== undefined && payload.capacity !== '') payload.capacity = Number(payload.capacity);
              const action = editItem
                ? tab === 0 ? await dispatch(updateRoute({ id: editItem.id, data: payload })) : await dispatch(updateVehicle({ id: editItem.id, data: payload }))
                : tab === 0 ? await dispatch(createRoute(payload)) : await dispatch(createVehicle(payload));
              const successAction = editItem ? (tab === 0 ? updateRoute : updateVehicle) : (tab === 0 ? createRoute : createVehicle);
              if (successAction.fulfilled.match(action)) {
                toast.success(`${tab === 0 ? 'Route' : 'Vehicle'} ${editItem ? 'updated' : 'created'}`);
                handleCloseDialog();
                const params = { page: page + 1, pageSize: rowsPerPage };
                if (tab === 0) dispatch(fetchRoutes(params));
                else dispatch(fetchVehicles(params));
              } else {
                toast.error(action.payload || 'Failed');
              }
            } finally { setSubmitting(false); }
          }}
        >
          {({ values, errors, touched, handleChange, handleBlur, isSubmitting }) => (
            <Form>
              <DialogContent>
                {tab === 0 ? (
                  <Grid container spacing={2}>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="name" label="Route Name" value={values.name}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.name && Boolean(errors.name)} helperText={touched.name && errors.name} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="startPoint" label="Start Location" value={values.startPoint}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.startPoint && Boolean(errors.startPoint)} helperText={touched.startPoint && errors.startPoint} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="endPoint" label="End Location" value={values.endPoint}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.endPoint && Boolean(errors.endPoint)} helperText={touched.endPoint && errors.endPoint} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="distance" label="Distance (km)" type="number" value={values.distance}
                        onChange={handleChange} onBlur={handleBlur} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="monthlyFee" label="Fare" type="number" value={values.monthlyFee}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.monthlyFee && Boolean(errors.monthlyFee)} helperText={touched.monthlyFee && errors.monthlyFee} />
                    </Grid>
                  </Grid>
                ) : (
                  <Grid container spacing={2}>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="registrationNumber" label="Registration Number" value={values.registrationNumber}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.registrationNumber && Boolean(errors.registrationNumber)} helperText={touched.registrationNumber && errors.registrationNumber} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth select name="vehicleType" label="Vehicle Type" value={values.vehicleType}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.vehicleType && Boolean(errors.vehicleType)} helperText={touched.vehicleType && errors.vehicleType}>
                        {VEHICLE_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                      </TextField>
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth select name="routeId" label="Route" value={values.routeId}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.routeId && Boolean(errors.routeId)} helperText={touched.routeId && errors.routeId}>
                        {(routes.items || []).map((r) => <MenuItem key={r.id} value={r.id}>{r.name}</MenuItem>)}
                      </TextField>
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="capacity" label="Capacity" type="number" value={values.capacity}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.capacity && Boolean(errors.capacity)} helperText={touched.capacity && errors.capacity} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="driverName" label="Driver Name" value={values.driverName}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.driverName && Boolean(errors.driverName)} helperText={touched.driverName && errors.driverName} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth select name="isActive" label="Status" value={values.isActive}
                        onChange={handleChange} onBlur={handleBlur}>
                        <MenuItem value={true}>Active</MenuItem>
                        <MenuItem value={false}>Inactive</MenuItem>
                      </TextField>
                    </Grid>
                  </Grid>
                )}
              </DialogContent>
              <DialogActions sx={{ px: 3, pb: 2 }}>
                <Button onClick={handleCloseDialog} variant="outlined">Cancel</Button>
                <Button type="submit" variant="contained" disabled={isSubmitting}>
                  {isSubmitting ? 'Saving...' : editItem ? 'Update' : 'Create'}
                </Button>
              </DialogActions>
            </Form>
          )}
        </Formik>
      </Dialog>
    </Box>
  );
}
