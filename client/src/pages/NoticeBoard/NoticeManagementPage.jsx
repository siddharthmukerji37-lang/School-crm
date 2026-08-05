import React, { useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box, Button, Chip, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Stack, Grid, MenuItem, FormControl, InputLabel, Select,
  Switch, FormControlLabel,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import {
  fetchNotices, createNotice, updateNotice, deleteNotice,
} from '../../store/slices/noticeSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

const priorityColors = {
  High: 'error',
  Medium: 'warning',
  Low: 'info',
};

export default function NoticeManagementPage() {
  const dispatch = useDispatch();
  const { notices, loading } = useSelector((state) => state.notices);

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editTarget, setEditTarget] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    title: '',
    content: '',
    type: 'Announcement',
    priority: 'Medium',
    publishDate: '',
    isPublished: true,
  });

  useEffect(() => {
    dispatch(fetchNotices({ page: page + 1, pageSize: rowsPerPage }));
  }, [dispatch, page, rowsPerPage]);

  const columns = [
    { id: 'title', header: 'Title', accessor: 'title', minWidth: 200 },
    {
      id: 'type', header: 'Type', accessor: 'type', minWidth: 120,
      render: (value) => (
        <Chip label={value || 'Announcement'} size="small" variant="outlined" />
      ),
    },
    {
      id: 'priority', header: 'Priority', accessor: 'priority', minWidth: 100,
      render: (value) => (
        <Chip
          label={value || 'Medium'}
          size="small"
          color={priorityColors[value] || 'default'}
        />
      ),
    },
    {
      id: 'publishDate', header: 'Publish Date', accessor: 'publishDate', minWidth: 120,
      render: (value) => value ? new Date(value).toLocaleDateString() : '—',
    },
    {
      id: 'status', header: 'Status', accessor: 'isPublished', minWidth: 100,
      render: (value) => (
        <Chip
          label={value ? 'Published' : 'Draft'}
          color={value ? 'success' : 'default'}
          size="small"
          variant="outlined"
        />
      ),
    },
    { id: 'createdByName', header: 'Created By', accessor: 'createdByName', minWidth: 140 },
  ];

  const handlePageChange = (_, newPage) => setPage(newPage);
  const handleRowsPerPageChange = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const resetForm = () => {
    setForm({ title: '', content: '', type: 'Announcement', priority: 'Medium', publishDate: '', isPublished: true });
    setEditTarget(null);
  };

  const handleOpenCreate = () => {
    resetForm();
    setDialogOpen(true);
  };

  const handleEdit = (row) => {
    setEditTarget(row);
    setForm({
      title: row.title || '',
      content: row.content || '',
      type: row.type || 'Announcement',
      priority: row.priority || 'Medium',
      publishDate: row.publishDate ? new Date(row.publishDate).toISOString().split('T')[0] : '',
      isPublished: row.isPublished ?? true,
    });
    setDialogOpen(true);
  };

  const handleDelete = (row) => setDeleteTarget(row);

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteNotice(deleteTarget.id));
    if (deleteNotice.fulfilled.match(result)) {
      toast.success('Notice deleted successfully');
      setDeleteTarget(null);
      dispatch(fetchNotices({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed to delete notice');
    }
  };

  const handleSubmit = async () => {
    if (!form.title.trim()) {
      toast.error('Title is required');
      return;
    }
    setSubmitting(true);
    try {
      const data = {
        ...form,
        publishDate: form.publishDate || new Date().toISOString(),
      };
      if (editTarget) {
        const result = await dispatch(updateNotice({ id: editTarget.id, data }));
        if (updateNotice.fulfilled.match(result)) {
          toast.success('Notice updated successfully');
        } else {
          toast.error(result.payload || 'Failed to update notice');
          return;
        }
      } else {
        const result = await dispatch(createNotice(data));
        if (createNotice.fulfilled.match(result)) {
          toast.success('Notice created successfully');
        } else {
          toast.error(result.payload || 'Failed to create notice');
          return;
        }
      }
      setDialogOpen(false);
      resetForm();
      dispatch(fetchNotices({ page: page + 1, pageSize: rowsPerPage }));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box>
      <PageHeader
        title="Notice Management"
        subtitle={`Total ${notices.totalCount || 0} notices`}
        actions={
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
            Add Notice
          </Button>
        }
      />

      <DataTable
        columns={columns}
        rows={notices.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={notices.totalCount || 0}
        searchPlaceholder="Search notices..."
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onEdit={handleEdit}
        onDelete={handleDelete}
        emptyMessage="No notices found"
      />

      <Dialog open={dialogOpen} onClose={() => { setDialogOpen(false); resetForm(); }} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>
          {editTarget ? 'Edit Notice' : 'Add New Notice'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <TextField
              label="Title" value={form.title}
              onChange={(e) => setForm((p) => ({ ...p, title: e.target.value }))}
              fullWidth required
            />
            <TextField
              label="Content" value={form.content}
              onChange={(e) => setForm((p) => ({ ...p, content: e.target.value }))}
              fullWidth multiline rows={4}
            />
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <FormControl fullWidth>
                  <InputLabel>Type</InputLabel>
                  <Select
                    value={form.type} label="Type"
                    onChange={(e) => setForm((p) => ({ ...p, type: e.target.value }))}
                  >
                    <MenuItem value="Announcement">Announcement</MenuItem>
                    <MenuItem value="Circular">Circular</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <FormControl fullWidth>
                  <InputLabel>Priority</InputLabel>
                  <Select
                    value={form.priority} label="Priority"
                    onChange={(e) => setForm((p) => ({ ...p, priority: e.target.value }))}
                  >
                    <MenuItem value="High">High</MenuItem>
                    <MenuItem value="Medium">Medium</MenuItem>
                    <MenuItem value="Low">Low</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
            <TextField
              label="Publish Date" type="date"
              value={form.publishDate}
              onChange={(e) => setForm((p) => ({ ...p, publishDate: e.target.value }))}
              InputLabelProps={{ shrink: true }} fullWidth
            />
            <FormControlLabel
              control={
                <Switch
                  checked={form.isPublished}
                  onChange={(e) => setForm((p) => ({ ...p, isPublished: e.target.checked }))}
                />
              }
              label="Published"
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2.5 }}>
          <Button onClick={() => { setDialogOpen(false); resetForm(); }} variant="outlined" disabled={submitting}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} variant="contained" disabled={submitting}>
            {submitting ? 'Saving...' : editTarget ? 'Update Notice' : 'Create Notice'}
          </Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Notice"
        message={`Are you sure you want to delete "${deleteTarget?.title || 'this notice'}"?`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  );
}
