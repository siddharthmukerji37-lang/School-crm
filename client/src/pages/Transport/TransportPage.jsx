import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box, Tabs, Tab, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Grid, Typography, IconButton, Stack,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
  fetchRoutes, createRoute, updateRoute, deleteRoute,
  fetchVehicles, createVehicle, updateVehicle, deleteVehicle,
  fetchAllocations,
} from '../../store/slices/transportSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import { useDispatch as useAppDispatch } from 'react-redux';
import toast from 'react-hot-toast';

const routeSchema = Yup.object({
  name: Yup.string().trim().required('Name is required'),
  startLocation: Yup.string().trim().required('Start location is required'),
  endLocation: Yup.string().trim().required('End location is required'),
  distance: Yup.number().transform((v, o) => o === '' ? undefined : v).min(0),
  fare: Yup.number().transform((v, o) => o === '' ? undefined : v).min(0).required('Fare is required'),
});

const vehicleSchema = Yup.object({
  registrationNumber: Yup.string().trim().required('Registration number is required'),
  vehicleType: Yup.string().required('Vehicle type is required'),
  capacity: Yup.number().transform((v, o) => o === '' ? undefined : v).min(1).required('Capacity is required'),
  driverName: Yup.string().trim().required('Driver name is required'),
  status: Yup.string(),
});

const VEHICLE_TYPES = ['Bus', 'Van', 'Car', 'Mini Bus'];

export default function TransportPage() {
  const dispatch = useDispatch();
  const { routes, vehicles, allocations } = useSelector((state) => state.transport);
  const { user } = useSelector((state) => state.auth);
  const userRole = user?.roles?.[0] || user?.role || 'Admin';
  const isAdmin = ['SuperAdmin', 'Admin'].includes(userRole);

  const [tab, setTab] = useState(0);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleteType, setDeleteType] = useState('');

  useEffect(() => {
    const params = { page: page + 1, pageSize: rowsPerPage };
    if (tab === 0) dispatch(fetchRoutes(params));
    else if (tab === 1) dispatch(fetchVehicles(params));
    else dispatch(fetchAllocations(params));
  }, [dispatch, tab, page, rowsPerPage]);

  const routeColumns = [
    { id: 'name', header: 'Route Name', accessor: 'name', minWidth: 160 },
    { id: 'startLocation', header: 'Start', accessor: 'startLocation', minWidth: 140 },
    { id: 'endLocation', header: 'End', accessor: 'endLocation', minWidth: 140 },
    { id: 'distance', header: 'Distance (km)', accessor: 'distance', minWidth: 110, align: 'center' },
    { id: 'fare', header: 'Fare', accessor: 'fare', minWidth: 100, align: 'right', render: (v) => `$${Number(v || 0).toFixed(2)}` },
  ];

  const vehicleColumns = [
    { id: 'registrationNumber', header: 'Reg. Number', accessor: 'registrationNumber', minWidth: 130 },
    { id: 'vehicleType', header: 'Type', accessor: 'vehicleType', minWidth: 100 },
    { id: 'capacity', header: 'Capacity', accessor: 'capacity', minWidth: 90, align: 'center' },
    { id: 'driverName', header: 'Driver', accessor: 'driverName', minWidth: 150 },
    { id: 'status', header: 'Status', accessor: 'status', minWidth: 100 },
  ];

  const allocationColumns = [
    { id: 'studentName', header: 'Student', accessor: 'studentName', minWidth: 160 },
    { id: 'routeName', header: 'Route', accessor: 'routeName', minWidth: 140 },
    { id: 'vehicleNumber', header: 'Vehicle', accessor: 'vehicleNumber', minWidth: 120 },
    { id: 'pickupPoint', header: 'Pickup Point', accessor: 'pickupPoint', minWidth: 130 },
    { id: 'monthlyFee', header: 'Monthly Fee', accessor: 'monthlyFee', minWidth: 110, render: (v) => `$${Number(v || 0).toFixed(2)}` },
  ];

  const handleOpenDialog = (item = null) => { setEditItem(item); setDialogOpen(true); };
  const handleCloseDialog = () => { setEditItem(null); setDialogOpen(false); };

  const handleDelete = (item, type) => { setDeleteTarget(item); setDeleteType(type); };
  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const action = deleteType === 'route'
      ? await dispatch(deleteRoute(deleteTarget.id))
      : await dispatch(deleteVehicle(deleteTarget.id));
    const successAction = deleteType === 'route' ? deleteRoute : deleteVehicle;
    if (successAction.fulfilled.match(action)) {
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

  const currentData = tab === 0 ? routes : tab === 1 ? vehicles : allocations;
  const currentColumns = tab === 0 ? routeColumns : tab === 1 ? vehicleColumns : allocationColumns;

  return (
    <Box>
      <PageHeader title="Transport Management" subtitle="Manage routes, vehicles and student allocations" />
      <Tabs value={tab} onChange={(_, v) => { setTab(v); setPage(0); }} sx={{ mb: 2 }}>
        <Tab label="Routes" />
        <Tab label="Vehicles" />
        <Tab label="Allocations" />
      </Tabs>

      {isAdmin && tab < 2 && (
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()}>
            Add {tab === 0 ? 'Route' : 'Vehicle'}
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
        onEdit={isAdmin && tab < 2 ? (row) => handleOpenDialog(row) : undefined}
        onDelete={isAdmin && tab < 2 ? (row) => handleDelete(row, tab === 0 ? 'route' : 'vehicle') : undefined}
        emptyMessage={`No ${tab === 0 ? 'routes' : tab === 1 ? 'vehicles' : 'allocations'} found`}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title={`Delete ${deleteType}`}
        message={`Are you sure you want to delete this ${deleteType}?`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => { setDeleteTarget(null); setDeleteType(''); }}
      />

      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editItem ? `Edit ${tab === 0 ? 'Route' : 'Vehicle'}` : `Add ${tab === 0 ? 'Route' : 'Vehicle'}`}</DialogTitle>
        <Formik
          initialValues={tab === 0
            ? { name: editItem?.name || '', startLocation: editItem?.startLocation || '', endLocation: editItem?.endLocation || '', distance: editItem?.distance ?? '', fare: editItem?.fare ?? '' }
            : { registrationNumber: editItem?.registrationNumber || '', vehicleType: editItem?.vehicleType || '', capacity: editItem?.capacity ?? '', driverName: editItem?.driverName || '', status: editItem?.status || 'Active' }
          }
          validationSchema={tab === 0 ? routeSchema : vehicleSchema}
          onSubmit={async (values, { setSubmitting }) => {
            try {
              const payload = { ...values };
              if (payload.distance !== undefined && payload.distance !== '') payload.distance = Number(payload.distance);
              if (payload.fare !== undefined && payload.fare !== '') payload.fare = Number(payload.fare);
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
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="startLocation" label="Start Location" value={values.startLocation}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.startLocation && Boolean(errors.startLocation)} helperText={touched.startLocation && errors.startLocation} />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="endLocation" label="End Location" value={values.endLocation}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.endLocation && Boolean(errors.endLocation)} helperText={touched.endLocation && errors.endLocation} />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="distance" label="Distance (km)" type="number" value={values.distance}
                        onChange={handleChange} onBlur={handleBlur} />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="fare" label="Fare" type="number" value={values.fare}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.fare && Boolean(errors.fare)} helperText={touched.fare && errors.fare} />
                    </Grid>
                  </Grid>
                ) : (
                  <Grid container spacing={2}>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="registrationNumber" label="Registration Number" value={values.registrationNumber}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.registrationNumber && Boolean(errors.registrationNumber)} helperText={touched.registrationNumber && errors.registrationNumber} />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth select name="vehicleType" label="Vehicle Type" value={values.vehicleType}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.vehicleType && Boolean(errors.vehicleType)} helperText={touched.vehicleType && errors.vehicleType}>
                        {VEHICLE_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
                      </TextField>
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="capacity" label="Capacity" type="number" value={values.capacity}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.capacity && Boolean(errors.capacity)} helperText={touched.capacity && errors.capacity} />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="driverName" label="Driver Name" value={values.driverName}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.driverName && Boolean(errors.driverName)} helperText={touched.driverName && errors.driverName} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="status" label="Status" value={values.status}
                        onChange={handleChange} onBlur={handleBlur} />
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
