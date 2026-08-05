import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip, MenuItem, TextField, Stack } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchExams, deleteExam } from '../../store/slices/examSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

export default function ExamListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { exams, loading } = useSelector((state) => state.exams);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin');

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);

  useEffect(() => {
    dispatch(fetchExams({ page: page + 1, pageSize: rowsPerPage }));
  }, [dispatch, page, rowsPerPage]);

  const columns = [
    { id: 'name', header: 'Exam Name', accessor: 'name', minWidth: 180 },
    { id: 'examType', header: 'Type', accessor: 'examType', minWidth: 110 },
    { id: 'className', header: 'Class', accessor: 'className', minWidth: 100 },
    { id: 'startDate', header: 'Start Date', accessor: 'startDate', minWidth: 110 },
    { id: 'endDate', header: 'End Date', accessor: 'endDate', minWidth: 110 },
    { id: 'totalMarks', header: 'Total Marks', accessor: 'totalMarks', minWidth: 100, align: 'center' },
    {
      id: 'status',
      header: 'Status',
      accessor: 'status',
      minWidth: 100,
      render: (value) => (
        <Chip
          label={value || 'Upcoming'}
          color={value === 'Completed' ? 'success' : value === 'Ongoing' ? 'warning' : 'info'}
          size="small"
          variant="outlined"
        />
      ),
    },
  ];

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteExam(deleteTarget.id));
    if (deleteExam.fulfilled.match(result)) {
      toast.success('Exam deleted successfully');
      setDeleteTarget(null);
      dispatch(fetchExams({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed to delete exam');
    }
  };

  return (
    <Box>
      <PageHeader
        title="Exams"
        subtitle={`Total ${exams.totalCount || 0} exams`}
        actions={
          isAdmin ? (
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
        onEdit={isAdmin ? (row) => navigate(`/exams/${row.id}/edit`) : undefined}
        onDelete={isAdmin ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No exams found"
      />
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Exam"
        message={`Are you sure you want to delete ${deleteTarget?.name || 'this exam'}?`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  );
}
