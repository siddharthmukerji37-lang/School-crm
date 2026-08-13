import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box, Tabs, Tab, Button, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Grid, Typography, Stack, MenuItem, Chip, CircularProgress,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { Formik, Form } from 'formik';
import * as Yup from 'yup';
import {
  fetchHostels, createHostel, updateHostel, deleteHostel,
  fetchAllRooms, createRoom, updateRoom, deleteRoom,
  fetchAllocations, allocateBed, checkout,
} from '../../store/slices/hostelSlice';
import { fetchStudents } from '../../store/slices/studentSlice';
import { findCurrentStudent, filterStudentAllocations } from '../../utils/studentAllocationUtils';
import hostelService from '../../services/hostelService';
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

const formatDate = (value) => {
  if (!value) return '-';
  const d = new Date(value);
  return isNaN(d.getTime()) ? '-' : d.toLocaleDateString();
};

export default function HostelPage() {
  const dispatch = useDispatch();
  const { hostels, rooms, allocations } = useSelector((state) => state.hostel);
  const { students } = useSelector((state) => state.students);
  const { user } = useSelector((state) => state.auth);
  const userRole = user?.roles?.[0] || user?.role || 'Admin';
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
  const [allocForm, setAllocForm] = useState({ studentId: '', roomId: '', bedId: '' });
  const [beds, setBeds] = useState([]);
  const [bedsLoading, setBedsLoading] = useState(false);
  const [allocSubmitting, setAllocSubmitting] = useState(false);
  const [checkoutTarget, setCheckoutTarget] = useState(null);

  useEffect(() => {
    const params = { page: page + 1, pageSize: rowsPerPage };

    if (isStudent) {
      dispatch(fetchAllocations({ ...params, studentId: currentStudent?.id }));
      return;
    }

    if (tab === 0) dispatch(fetchHostels(params));
    else if (tab === 1) {
      dispatch(fetchAllRooms());
      dispatch(fetchHostels({ page: 1, pageSize: 1000 }));
    } else {
      dispatch(fetchAllocations(params));
      dispatch(fetchAllRooms());
      dispatch(fetchHostels({ page: 1, pageSize: 1000 }));
    }
  }, [dispatch, isStudent, currentStudent?.id, tab, page, rowsPerPage]);

  useEffect(() => {
    dispatch(fetchStudents({ page: 1, pageSize: 500 }));
  }, [dispatch]);

  useEffect(() => {
    if (!allocForm.roomId) {
      setBeds([]);
      setAllocForm((f) => ({ ...f, bedId: '' }));
      return;
    }
    setBedsLoading(true);
    hostelService.getBeds(allocForm.roomId)
      .then((res) => {
        const allBeds = (res.data?.data || []).filter((b) => !b.isOccupied);
        setBeds(allBeds);
        setAllocForm((f) => ({ ...f, bedId: '' }));
      })
      .catch(() => setBeds([]))
      .finally(() => setBedsLoading(false));
  }, [allocForm.roomId]);

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
    { id: 'totalBeds', header: 'Capacity', accessor: 'totalBeds', minWidth: 80, align: 'center' },
    { id: 'availableBeds', header: 'Available', accessor: 'availableBeds', minWidth: 90, align: 'center' },
    { id: 'monthlyFee', header: 'Rent', accessor: 'monthlyFee', minWidth: 100, render: (v) => `$${Number(v || 0).toFixed(2)}` },
  ];

  const allocationColumns = [
    { id: 'studentName', header: 'Student', accessor: 'studentName', minWidth: 160 },
    { id: 'roomNumber', header: 'Room', accessor: 'roomNumber', minWidth: 100 },
    { id: 'hostelName', header: 'Hostel', accessor: 'hostelName', minWidth: 140 },
    { id: 'allocationDate', header: 'Allocated', accessor: 'allocationDate', minWidth: 110, render: formatDate },
    {
      id: 'status', header: 'Status', accessor: 'status', minWidth: 110,
      render: (v, row) => {
        const active = row.isActive || v === 'Active';
        return (
          <Chip
            label={active ? 'Active' : 'Checked Out'}
            color={active ? 'success' : 'default'}
            size="small"
            variant="outlined"
          />
        );
      },
    },
  ];

  const handleOpenDialog = (item = null) => { setEditItem(item); setDialogOpen(true); };
  const handleCloseDialog = () => { setEditItem(null); setDialogOpen(false); };

  const refreshAfterMutation = () => {
    if (page === 0) {
      if (tab === 0) dispatch(fetchHostels({ page: 1, pageSize: rowsPerPage }));
      else if (tab === 1) {
        dispatch(fetchAllRooms());
        dispatch(fetchHostels({ page: 1, pageSize: 1000 }));
      } else {
        dispatch(fetchAllocations({ page: 1, pageSize: rowsPerPage }));
        dispatch(fetchAllRooms());
        dispatch(fetchHostels({ page: 1, pageSize: 1000 }));
      }
    } else {
      setPage(0);
    }
  };

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
      refreshAfterMutation();
    } else {
      toast.error(action.payload || 'Failed');
    }
  };

  const handleAllocate = async () => {
    if (!allocForm.studentId || !allocForm.bedId) {
      toast.error('Select a student, room and bed');
      return;
    }
    setAllocSubmitting(true);
    try {
      const result = await dispatch(allocateBed({
        studentId: allocForm.studentId,
        bedId: allocForm.bedId,
      }));
      if (allocateBed.fulfilled.match(result)) {
        toast.success('Bed allocated');
        setAllocOpen(false);
        setAllocForm({ studentId: '', roomId: '', bedId: '' });
        setBeds([]);
        refreshAfterMutation();
      } else {
        toast.error(result.payload || 'Failed to allocate');
      }
    } finally {
      setAllocSubmitting(false);
    }
  };

  const confirmCheckout = async () => {
    if (!checkoutTarget) return;
    const result = await dispatch(checkout(checkoutTarget.id));
    if (checkout.fulfilled.match(result)) {
      toast.success('Student checked out');
      setCheckoutTarget(null);
      refreshAfterMutation();
    } else {
      toast.error(result.payload || 'Failed');
    }
  };

  const studentScopedAllocations = isStudent
    ? filterStudentAllocations(allocations?.items || [], currentStudent)
    : allocations?.items || [];
  const currentData = isStudent
    ? { items: studentScopedAllocations, totalCount: studentScopedAllocations.length }
    : tab === 0 ? hostels : tab === 1 ? rooms : allocations;
  const currentColumns = isStudent ? allocationColumns : tab === 0 ? hostelColumns : tab === 1 ? roomColumns : allocationColumns;
  const hostelOptions = hostels?.items || hostels || [];

  return (
    <Box>
      <PageHeader title="Hostel Management" subtitle={isStudent ? 'Your hostel allocation' : 'Manage hostels, rooms and allocations'} />
      <Tabs value={isStudent ? 2 : tab} onChange={(_, v) => { setTab(v); setPage(0); }} sx={{ mb: 2 }}>
        {!isStudent && <Tab label="Hostels" />}
        {!isStudent && <Tab label="Rooms" />}
        <Tab label={isStudent ? 'My Allocation' : 'Allocations'} />
      </Tabs>

      {!isStudent && (
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => (tab === 2 ? (() => { setAllocForm({ studentId: '', roomId: '', bedId: '' }); setBeds([]); setAllocOpen(true); })() : handleOpenDialog())}
          >
            {tab === 0 ? 'Add Hostel' : tab === 1 ? 'Add Room' : 'Allocate Bed'}
          </Button>
        </Box>
      )}

      <DataTable
        columns={currentColumns}
        rows={currentData?.items || (Array.isArray(currentData) ? currentData : [])}
        loading={false}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={currentData?.totalCount || 0}
        onPageChange={(_, p) => setPage(p)}
        onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
        onView={(row) => setViewItem(row)}
        onEdit={!isStudent && tab < 2 ? (row) => handleOpenDialog(row) : undefined}
        onDelete={!isStudent && tab < 2 ? (row) => handleDelete(row, tab === 0 ? 'hostel' : 'room') : undefined}
        onReturn={!isStudent && tab === 2 ? (row) => setCheckoutTarget(row) : undefined}
        emptyMessage={isStudent ? 'No hostel allocation found' : `No ${tab === 0 ? 'hostels' : tab === 1 ? 'rooms' : 'allocations'} found`}
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
        open={!!checkoutTarget}
        title="Check Out"
        message={`Check out "${checkoutTarget?.studentName}" from room ${checkoutTarget?.roomNumber}?`}
        confirmText="Check Out"
        onConfirm={confirmCheckout}
        onCancel={() => setCheckoutTarget(null)}
      />

      <Dialog open={!!viewItem} onClose={() => setViewItem(null)} maxWidth="sm" fullWidth>
        <DialogTitle>{tab === 0 ? 'Hostel Details' : tab === 1 ? 'Room Details' : 'Allocation Details'}</DialogTitle>
        <DialogContent dividers>
          {tab === 0 && viewItem && (
            <Stack spacing={1.5}>
              <Typography><b>Hostel Name:</b> {viewItem.name}</Typography>
              <Typography><b>Type:</b> {viewItem.type}</Typography>
              <Typography><b>Address:</b> {viewItem.address || '-'}</Typography>
              <Typography><b>Warden:</b> {viewItem.wardenName || '-'}{viewItem.wardenPhone ? ` (${viewItem.wardenPhone})` : ''}</Typography>
              <Typography><b>Rooms:</b> {viewItem.totalRooms ?? 0}</Typography>
              <Typography><b>Beds:</b> {viewItem.totalBeds ?? 0}</Typography>
              <Typography><b>Status:</b> {viewItem.isActive ? 'Active' : 'Inactive'}</Typography>
            </Stack>
          )}
          {tab === 1 && viewItem && (
            <Stack spacing={1.5}>
              <Typography><b>Room No.:</b> {viewItem.roomNumber}</Typography>
              <Typography><b>Hostel:</b> {viewItem.hostelName || '-'}</Typography>
              <Typography><b>Type:</b> {viewItem.roomType}</Typography>
              <Typography><b>Capacity:</b> {viewItem.totalBeds}</Typography>
              <Typography><b>Available:</b> {viewItem.availableBeds}</Typography>
              <Typography><b>Rent:</b> ${Number(viewItem.monthlyFee || 0).toFixed(2)}</Typography>
            </Stack>
          )}
          {tab === 2 && viewItem && (
            <Stack spacing={1.5}>
              <Typography><b>Student:</b> {viewItem.studentName}</Typography>
              <Typography><b>Hostel:</b> {viewItem.hostelName || '-'}</Typography>
              <Typography><b>Room:</b> {viewItem.roomNumber}</Typography>
              <Typography><b>Allocated:</b> {formatDate(viewItem.allocationDate)}</Typography>
              <Typography><b>Checked Out:</b> {formatDate(viewItem.deallocationDate)}</Typography>
              <Typography><b>Status:</b> {viewItem.isActive ? 'Active' : 'Checked Out'}</Typography>
            </Stack>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewItem(null)} variant="outlined">Close</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={allocOpen} onClose={() => setAllocOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Allocate Bed</DialogTitle>
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
              label="Select Room"
              value={allocForm.roomId}
              onChange={(e) => setAllocForm({ ...allocForm, roomId: e.target.value })}
            >
              {(Array.isArray(rooms) ? rooms : [])
                .filter((r) => r.availableBeds > 0)
                .map((r) => (
                  <MenuItem key={r.id} value={r.id}>
                    {r.roomNumber} ({r.hostelName || 'No hostel'} - {r.availableBeds} available)
                  </MenuItem>
                ))}
            </TextField>
            <TextField
              select
              fullWidth
              label="Select Bed"
              value={allocForm.bedId}
              onChange={(e) => setAllocForm({ ...allocForm, bedId: e.target.value })}
              disabled={!allocForm.roomId || bedsLoading}
            >
              {bedsLoading && <MenuItem value="">Loading beds...</MenuItem>}
              {!bedsLoading && beds.length === 0 && <MenuItem value="">No beds available</MenuItem>}
              {!bedsLoading && beds.map((b) => (
                <MenuItem key={b.id} value={b.id}>{b.bedNumber}</MenuItem>
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
        <DialogTitle>{editItem ? `Edit ${tab === 0 ? 'Hostel' : 'Room'}` : `Add ${tab === 0 ? 'Hostel' : 'Room'}`}</DialogTitle>
        <Formik
          key={tab === 0 ? `hostel-${editItem?.id || 'new'}` : `room-${editItem?.id || 'new'}`}
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
                refreshAfterMutation();
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
                        {hostelOptions.map((h) => (
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
