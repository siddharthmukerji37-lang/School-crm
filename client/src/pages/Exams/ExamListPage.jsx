import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box, Button, Chip, Stack, Dialog, DialogTitle, DialogContent, DialogActions,
  TextField, Typography,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import TaskAltIcon from '@mui/icons-material/TaskAlt';
import CancelIcon from '@mui/icons-material/Cancel';
import QuizIcon from '@mui/icons-material/Quiz';
import EditNoteIcon from '@mui/icons-material/EditNote';
import { fetchExams, deleteExam } from '../../store/slices/examSlice';
import examService from '../../services/examService';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

const approvalColor = (s) => {
  switch (s) {
    case 'Approved': return 'success';
    case 'Rejected': return 'error';
    default: return 'warning';
  }
};

export default function ExamListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { exams, loading } = useSelector((state) => state.exams);
  const { user } = useSelector((state) => state.auth);
  const roles = user?.roles || [];
  const isAdmin = roles.some((r) => r === 'SuperAdmin' || r === 'Admin');
  const isTeacher = roles.some((r) => r === 'Teacher' || r === 'ClassTeacher');
  const isStudent = roles.some((r) => r === 'Student');
  const canCreate = isAdmin || isTeacher;

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [approvalTarget, setApprovalTarget] = useState(null);
  const [approvalApproved, setApprovalApproved] = useState(true);
  const [approvalReason, setApprovalReason] = useState('');
  const [processing, setProcessing] = useState(false);

  useEffect(() => {
    dispatch(fetchExams({ pageNumber: page + 1, pageSize: rowsPerPage }));
  }, [dispatch, page, rowsPerPage]);

  const columns = [
    { id: 'name', header: 'Exam Name', accessor: 'name', minWidth: 180 },
    { id: 'examType', header: 'Type', accessor: 'examType', minWidth: 110 },
    { id: 'className', header: 'Class', accessor: 'className', minWidth: 100 },
    {
      id: 'approvalStatus', header: 'Approval', accessor: 'approvalStatus', minWidth: 110,
      render: (v, row) => (
        <Stack direction="column" spacing={0.5}>
          <Chip label={v || 'Pending'} color={approvalColor(v)} size="small" variant="outlined" />
          {row.rejectionReason && (
            <Typography variant="caption" color="error" sx={{ maxWidth: 140 }}>
              {row.rejectionReason}
            </Typography>
          )}
        </Stack>
      ),
    },
    {
      id: 'questions', header: 'Questions', accessor: 'questionCount', minWidth: 90, align: 'center',
      render: (v) => <Typography variant="body2">{v ?? 0}</Typography>,
    },
    {
      id: 'totalMarks', header: 'Total Marks', accessor: 'totalMarks', minWidth: 90, align: 'center',
    },
    { id: 'startDate', header: 'Start', accessor: 'startDate', minWidth: 110 },
    {
      id: 'actions', header: 'Actions', accessor: 'id', minWidth: 240, sortable: false,
      render: (v, row) => renderActions(row),
    },
  ];

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteExam(deleteTarget.id));
    if (deleteExam.fulfilled.match(result)) {
      toast.success('Exam deleted successfully');
      setDeleteTarget(null);
      dispatch(fetchExams({ pageNumber: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed to delete exam');
    }
  };

  const openApprove = (row, approved) => {
    setApprovalTarget(row);
    setApprovalApproved(approved);
    setApprovalReason('');
  };

  const confirmApproval = async () => {
    if (!approvalTarget) return;
    setProcessing(true);
    try {
      await examService.approveExam(approvalTarget.id, {
        approved: approvalApproved,
        reason: approvalApproved ? null : approvalReason,
      });
      toast.success(approvalApproved ? 'Exam approved' : 'Exam rejected');
      setApprovalTarget(null);
      dispatch(fetchExams({ pageNumber: page + 1, pageSize: rowsPerPage }));
    } catch (e) {
      toast.error(e.message || 'Failed to update approval');
    } finally {
      setProcessing(false);
    }
  };

  const renderActions = (row) => {
    if (isStudent) {
      return row.approvalStatus === 'Approved' ? (
        <Button size="small" variant="contained" onClick={() => navigate(`/exams/${row.id}/take`)}>Take Exam</Button>
      ) : (
        <Chip label="Not available" size="small" />
      );
    }
    return (
      <Stack direction="row" spacing={1}>
        <Button
          size="small" variant="outlined" startIcon={<EditNoteIcon />}
          onClick={() => navigate(`/exams/${row.id}/questions`)}
        >
          Questions
        </Button>
        {(isAdmin || isTeacher) && (
          <Button
            size="small" variant="outlined" startIcon={<QuizIcon />}
            onClick={() => navigate(`/exams/${row.id}/submissions`)}
          >
            Submissions
          </Button>
        )}
        {isAdmin && (row.approvalStatus === 'Pending' || row.approvalStatus === 'Rejected' || row.approvalStatus === '0') && (
          <Stack direction="row" spacing={0.5}>
            <Button size="small" variant="contained" color="success" startIcon={<TaskAltIcon />}
              onClick={() => openApprove(row, true)}>
              Approve
            </Button>
            <Button size="small" variant="outlined" color="error" startIcon={<CancelIcon />}
              onClick={() => openApprove(row, false)}>
              Reject
            </Button>
          </Stack>
        )}
      </Stack>
    );
  };

  return (
    <Box>
      <PageHeader
        title="Exams"
        subtitle={`Total ${exams.totalCount || 0} exams`}
        actions={
          canCreate ? (
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/exams/create')}>
              Add Exam
            </Button>
          ) : null
        }
      />
      <DataTable
        columns={columns}
        rows={exams.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={exams.totalCount || 0}
        searchPlaceholder="Search exams..."
        onPageChange={(_, p) => setPage(p)}
        onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
        emptyMessage="No exams found"
        showActions={false}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Exam"
        message={`Are you sure you want to delete ${deleteTarget?.name || 'this exam'}?`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />

      <Dialog open={!!approvalTarget} onClose={() => setApprovalTarget(null)} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>
          {approvalApproved ? 'Approve Exam' : 'Reject Exam'}
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            {approvalTarget?.name}
          </Typography>
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
