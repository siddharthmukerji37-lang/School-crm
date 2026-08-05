import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box, Tabs, Tab, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Grid, Typography, MenuItem,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
  fetchHostels, createHostel, updateHostel, deleteHostel,
  fetchRooms, createRoom, updateRoom, deleteRoom,
  fetchAllocations,
} from '../../store/slices/hostelSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

const hostelSchema = Yup.object({
  name: Yup.string().trim().required('Name is required'),
  type: Yup.string().required('Type is required'),
  address: Yup.string().trim(),
  wardenName: Yup.string().trim(),
});

const roomSchema = Yup.object({
  hostelId: Yup.string().required('Hostel is required'),
  roomNumber: Yup.string().trim().required('Room number is required'),
  roomType: Yup.string().required('Room type is required'),
  totalBeds: Yup.number().transform((v, o) => o === '' ? undefined : v).min(1).required('Total beds is required'),
  monthlyFee: Yup.number().transform((v, o) => o === '' ? undefined : v).min(0).required('Monthly fee is required'),
});

const HOSTEL_TYPES = ['Boys', 'Girls', 'Co-ed'];
const ROOM_TYPES = ['Single', 'Double', 'Triple', 'Dormitory', 'Suite'];

export default function HostelPage() {
  const dispatch = useDispatch();
  const { hostels, rooms, allocations } = useSelector((state) => state.hostel);

  const [tab, setTab] = useState(0);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [deleteType, setDeleteType] = useState('');

  useEffect(() => {
    const params = { page: page + 1, pageSize: rowsPerPage };
    if (tab === 0) dispatch(fetchHostels(params));
    else if (tab === 1) dispatch(fetchHostels({ page: 1, pageSize: 1000 }));
    else if (tab === 2) dispatch(fetchAllocations(params));
  }, [dispatch, tab, page, rowsPerPage]);

  const hostelColumns = [
    { id: 'name', header: 'Hostel Name', accessor: 'name', minWidth: 160 },
    { id: 'type', header: 'Type', accessor: 'type', minWidth: 100 },
    { id: 'address', header: 'Address', accessor: 'address', minWidth: 180 },
    { id: 'totalRooms', header: 'Rooms', accessor: 'totalRooms', minWidth: 80, align: 'center' },
    { id: 'totalBeds', header: 'Beds', accessor: 'totalBeds', minWidth: 80, align: 'center' },
    { id: 'wardenName', header: 'Warden', accessor: 'wardenName', minWidth: 140 },
  ];

  const roomColumns = [
    { id: 'roomNumber', header: 'Room No.', accessor: 'roomNumber', minWidth: 100 },
    { id: 'hostelName', header: 'Hostel', accessor: 'hostelName', minWidth: 140 },
    { id: 'roomType', header: 'Type', accessor: 'roomType', minWidth: 100 },
    { id: 'capacity', header: 'Capacity', accessor: 'capacity', minWidth: 80, align: 'center' },
    { id: 'availableBeds', header: 'Available', accessor: 'availableBeds', minWidth: 90, align: 'center' },
    { id: 'monthlyFee', header: 'Rent', accessor: 'monthlyFee', minWidth: 100, render: (v) => `$${Number(v || 0).toFixed(2)}` },
  ];

  const allocationColumns = [
    { id: 'studentName', header: 'Student', accessor: 'studentName', minWidth: 160 },
    { id: 'roomNumber', header: 'Room', accessor: 'roomNumber', minWidth: 100 },
    { id: 'hostelName', header: 'Hostel', accessor: 'hostelName', minWidth: 140 },
    { id: 'allocatedDate', header: 'Allocated', accessor: 'allocatedDate', minWidth: 110 },
    { id: 'status', header: 'Status', accessor: 'status', minWidth: 100 },
  ];

  const handleOpenDialog = (item = null) => { setEditItem(item); setDialogOpen(true); };
  const handleCloseDialog = () => { setEditItem(null); setDialogOpen(false); };

  const handleDelete = (item, type) => { setDeleteTarget(item); setDeleteType(type); };
  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const action = deleteType === 'hostel'
      ? await dispatch(deleteHostel(deleteTarget.id))
      : await dispatch(deleteRoom(deleteTarget.id));
    const successAction = deleteType === 'hostel' ? deleteHostel : deleteRoom;
    if (successAction.fulfilled.match(action)) {
      toast.success(`${deleteType} deleted`);
      setDeleteTarget(null);
      setDeleteType('');
      const params = { page: page + 1, pageSize: rowsPerPage };
      if (tab === 0) dispatch(fetchHostels(params));
    } else {
      toast.error(action.payload || 'Failed');
    }
  };

  const currentData = tab === 0 ? hostels : tab === 1 ? rooms : allocations;
  const currentColumns = tab === 0 ? hostelColumns : tab === 1 ? roomColumns : allocationColumns;

  return (
    <Box>
      <PageHeader title="Hostel Management" subtitle="Manage hostels, rooms and allocations" />
      <Tabs value={tab} onChange={(_, v) => { setTab(v); setPage(0); }} sx={{ mb: 2 }}>
        <Tab label="Hostels" />
        <Tab label="Rooms" />
        <Tab label="Allocations" />
      </Tabs>

      {tab < 2 && (
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpenDialog()}>
            Add {tab === 0 ? 'Hostel' : 'Room'}
          </Button>
        </Box>
      )}

      <DataTable
        columns={currentColumns}
        rows={currentData?.items || currentData || []}
        loading={false}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={currentData?.totalCount || 0}
        onPageChange={(_, p) => setPage(p)}
        onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
        onEdit={tab < 2 ? (row) => handleOpenDialog(row) : undefined}
        onDelete={tab < 2 ? (row) => handleDelete(row, tab === 0 ? 'hostel' : 'room') : undefined}
        emptyMessage={`No ${tab === 0 ? 'hostels' : tab === 1 ? 'rooms' : 'allocations'} found`}
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
        <DialogTitle>{editItem ? `Edit ${tab === 0 ? 'Hostel' : 'Room'}` : `Add ${tab === 0 ? 'Hostel' : 'Room'}`}</DialogTitle>
        <Formik
          initialValues={tab === 0
            ? { name: editItem?.name || '', type: editItem?.type || '', address: editItem?.address || '', wardenName: editItem?.wardenName || '' }
            : { hostelId: editItem?.hostelId || '', roomNumber: editItem?.roomNumber || '', roomType: editItem?.roomType || '', totalBeds: editItem?.totalBeds ?? '', monthlyFee: editItem?.monthlyFee ?? editItem?.rentAmount ?? '' }
          }
          validationSchema={tab === 0 ? hostelSchema : roomSchema}
          onSubmit={async (values, { setSubmitting }) => {
            try {
              const payload = { ...values };
              if (payload.totalBeds !== undefined && payload.totalBeds !== '') payload.totalBeds = Number(payload.totalBeds);
              if (payload.monthlyFee !== undefined && payload.monthlyFee !== '') payload.monthlyFee = Number(payload.monthlyFee);
              const action = editItem
                ? tab === 0 ? await dispatch(updateHostel({ id: editItem.id, data: payload })) : await dispatch(updateRoom({ id: editItem.id, data: payload }))
                : tab === 0 ? await dispatch(createHostel(payload)) : await dispatch(createRoom(payload));
              const successAction = editItem ? (tab === 0 ? updateHostel : updateRoom) : (tab === 0 ? createHostel : createRoom);
              if (successAction.fulfilled.match(action)) {
                toast.success(`${tab === 0 ? 'Hostel' : 'Room'} ${editItem ? 'updated' : 'created'}`);
                handleCloseDialog();
                const params = { page: page + 1, pageSize: rowsPerPage };
                if (tab === 0) dispatch(fetchHostels(params));
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
                      <TextField fullWidth name="name" label="Hostel Name" value={values.name}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.name && Boolean(errors.name)} helperText={touched.name && errors.name} />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth select name="type" label="Type" value={values.type}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.type && Boolean(errors.type)} helperText={touched.type && errors.type}>
                        {HOSTEL_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                      </TextField>
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="wardenName" label="Warden Name" value={values.wardenName}
                        onChange={handleChange} onBlur={handleBlur} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="address" label="Address" value={values.address}
                        onChange={handleChange} onBlur={handleBlur} />
                    </Grid>
                  </Grid>
                ) : (
                  <Grid container spacing={2}>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth select name="hostelId" label="Hostel" value={values.hostelId}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.hostelId && Boolean(errors.hostelId)} helperText={touched.hostelId && errors.hostelId}>
                        {(hostels?.items || hostels || []).map((h) => (
                          <MenuItem key={h.id} value={h.id}>{h.name}</MenuItem>
                        ))}
                      </TextField>
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="roomNumber" label="Room Number" value={values.roomNumber}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.roomNumber && Boolean(errors.roomNumber)} helperText={touched.roomNumber && errors.roomNumber} />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth select name="roomType" label="Room Type" value={values.roomType}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.roomType && Boolean(errors.roomType)} helperText={touched.roomType && errors.roomType}>
                        {ROOM_TYPES.map((t) => <MenuItem key={t} value={t}>{t}</MenuItem>)}
                      </TextField>
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <TextField fullWidth name="totalBeds" label="Total Beds" type="number" value={values.totalBeds}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.totalBeds && Boolean(errors.totalBeds)} helperText={touched.totalBeds && errors.totalBeds} />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                      <TextField fullWidth name="monthlyFee" label="Monthly Fee" type="number" value={values.monthlyFee}
                        onChange={handleChange} onBlur={handleBlur}
                        error={touched.monthlyFee && Boolean(errors.monthlyFee)} helperText={touched.monthlyFee && errors.monthlyFee} />
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
