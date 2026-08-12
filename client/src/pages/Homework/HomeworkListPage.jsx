import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip, Stack, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Typography } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import TaskAltIcon from '@mui/icons-material/TaskAlt';
import CancelIcon from '@mui/icons-material/Cancel';
import ThumbUpIcon from '@mui/icons-material/ThumbUp';
import { fetchHomework, deleteHomework } from '../../store/slices/homeworkSlice';
import homeworkService from '../../services/homeworkService';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

const statusColor = (s) => {
  switch (s) { case 'Completed': return 'success'; case 'Overdue': return 'error'; case 'Pending': return 'warning'; case 'Assigned': return 'info'; default: return 'default'; }
};

const approvalColor = (s) => {
  switch (s) { case 'Approved': return 'success'; case 'Rejected': return 'error'; default: return 'warning'; }
};

export default function HomeworkListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { homework, loading } = useSelector((state) => state.homework);
  const { user } = useSelector((state) => state.auth);
  const roles = user?.roles || [];
  const isAdmin = roles.some((r) => r === 'SuperAdmin' || r === 'Admin');
  const isTeacher = roles.some((r) => r === 'Teacher' || r === 'ClassTeacher');
  const canManage = isAdmin || isTeacher;

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [approvalTarget, setApprovalTarget] = useState(null);
  const [approvalApproved, setApprovalApproved] = useState(true);
  const [approvalReason, setApprovalReason] = useState('');
  const [processing, setProcessing] = useState(false);

  useEffect(() => {
    dispatch(fetchHomework({ page: page + 1, pageSize: rowsPerPage }));
  }, [dispatch, page, rowsPerPage]);

  const reload = () => dispatch(fetchHomework({ page: page + 1, pageSize: rowsPerPage }));

  const columns = [
    { id: 'title', header: 'Title', accessor: 'title', minWidth: 180 },
    { id: 'subject', header: 'Subject', accessor: 'subjectName', minWidth: 120 },
    { id: 'class', header: 'Class', accessor: 'className', minWidth: 90 },
    { id: 'section', header: 'Section', accessor: 'sectionName', minWidth: 80 },
    { id: 'dueDate', header: 'Due Date', accessor: 'dueDate', minWidth: 110 },
    {
      id: 'approval', header: 'Approval', accessor: 'approvalStatus', minWidth: 110,
      render: (v, row) => (
        <Stack direction="column" spacing={0.5}>
          <Chip label={v || 'Pending'} color={approvalColor(v)} size="small" variant="outlined" />
          {row.rejectionReason && (
            <Typography variant="caption" color="error" sx={{ maxWidth: 140 }}>{row.rejectionReason}</Typography>
          )}
        </Stack>
      ),
    },
    {
      id: 'status', header: 'Status', accessor: 'status', minWidth: 100,
      render: (v) => <Chip label={v || 'Pending'} color={statusColor(v)} size="small" variant="outlined" />,
    },
    {
      id: 'actions', header: 'Actions', accessor: 'id', minWidth: 260, sortable: false,
      render: (v, row) => {
        const notApproved = row.approvalStatus !== 'Approved';
        return (
          <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
            <Button size="small" variant="outlined" onClick={() => navigate(`/homework/${row.id}`)}>View</Button>
            {isAdmin && (
              <>
                <Button size="small" variant="outlined" onClick={() => navigate(`/homework/${row.id}/edit`)}>Edit</Button>
                <Button size="small" variant="outlined" color="error" onClick={() => setDeleteTarget(row)}>Delete</Button>
              </>
            )}
            {isAdmin && notApproved && (
              <Button size="small" variant="contained" color="success" startIcon={<TaskAltIcon />}
                onClick={() => { setApprovalTarget(row); setApprovalApproved(true); setApprovalReason(''); }}>
                Approve
              </Button>
            )}
            {isAdmin && notApproved && (
              <Button size="small" variant="outlined" color="error" startIcon={<CancelIcon />}
                onClick={() => { setApprovalTarget(row); setApprovalApproved(false); setApprovalReason(''); }}>
                Reject
              </Button>
            )}
            {isTeacher && notApproved && (
              <Button size="small" variant="contained" startIcon={<ThumbUpIcon />}
                onClick={async () => {
                  try {
                    await homeworkService.submitForApproval(row.id);
                    toast.success('Submitted for approval');
                    reload();
                  } catch (e) { toast.error(e.message || 'Failed'); }
                }}>
                Request Approval
              </Button>
            )}
          </Stack>
        );
      },
    },
  ];

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteHomework(deleteTarget.id));
    if (deleteHomework.fulfilled.match(result)) {
      toast.success('Homework deleted');
      setDeleteTarget(null);
      reload();
    } else {
      toast.error(result.payload || 'Failed to delete');
    }
  };

  const confirmApproval = async () => {
    if (!approvalTarget) return;
    setProcessing(true);
    try {
      await homeworkService.approve(approvalTarget.id, {
        approved: approvalApproved,
        reason: approvalApproved ? null : approvalReason,
      });
      toast.success(approvalApproved ? 'Homework approved' : 'Homework rejected');
      setApprovalTarget(null);
      reload();
    } catch (e) {
      toast.error(e.message || 'Failed to update approval');
    } finally {
      setProcessing(false);
    }
  };

  return (
    <Box>
      <PageHeader
        title="Homework"
        subtitle={`Total ${homework.totalCount || 0} assignments`}
        actions={
          canManage ? (
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/homework/create')}>
              Add Homework
            </Button>
          ) : null
        }
      />
      <DataTable
        columns={columns}
        rows={homework.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={homework.totalCount || 0}
        searchPlaceholder="Search homework..."
        onPageChange={(_, p) => setPage(p)}
        onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
        onView={(row) => navigate(`/homework/${row.id}`)}
        emptyMessage="No homework found"
      />
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Homework"
        message={`Delete "${deleteTarget?.title}"?`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />

      <Dialog open={!!approvalTarget} onClose={() => setApprovalTarget(null)} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>
          {approvalApproved ? 'Approve Homework' : 'Reject Homework'}
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>{approvalTarget?.title}</Typography>
          {!approvalApproved && (
            <TextField
              fullWidth label="Reason for rejection" multiline rows={2} size="small"
              value={approvalReason}
              onChange={(e) => setApprovalReason(e.target.value)}
            />
          )}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2.5 }}>
          <Button onClick={() => setApprovalTarget(null)} variant="outlined">Cancel</Button>
          <Button
            onClick={confirmApproval}
            color={approvalApproved ? 'success' : 'error'}
            variant="contained"
            disabled={processing || (!approvalApproved && !approvalReason.trim())}
          >
            {processing ? 'Saving...' : approvalApproved ? 'Approve' : 'Reject'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
