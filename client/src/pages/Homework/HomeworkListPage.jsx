import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip, TextField, MenuItem, Stack } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchHomework, deleteHomework } from '../../store/slices/homeworkSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

const statusColor = (s) => {
  switch (s) { case 'Completed': return 'success'; case 'Overdue': return 'error'; case 'Pending': return 'warning'; default: return 'default'; }
};

export default function HomeworkListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { homework, loading } = useSelector((state) => state.homework);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin' || r === 'Teacher');

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);

  useEffect(() => {
    dispatch(fetchHomework({ page: page + 1, pageSize: rowsPerPage }));
  }, [dispatch, page, rowsPerPage]);

  const columns = [
    { id: 'title', header: 'Title', accessor: 'title', minWidth: 200 },
    { id: 'subject', header: 'Subject', accessor: 'subjectName', minWidth: 130 },
    { id: 'class', header: 'Class', accessor: 'className', minWidth: 100 },
    { id: 'section', header: 'Section', accessor: 'sectionName', minWidth: 90 },
    { id: 'dueDate', header: 'Due Date', accessor: 'dueDate', minWidth: 110 },
    { id: 'assignedBy', header: 'Assigned By', accessor: 'assignedBy', minWidth: 130 },
    {
      id: 'status', header: 'Status', accessor: 'status', minWidth: 100,
      render: (v) => <Chip label={v || 'Pending'} color={statusColor(v)} size="small" variant="outlined" />,
    },
  ];

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteHomework(deleteTarget.id));
    if (deleteHomework.fulfilled.match(result)) {
      toast.success('Homework deleted');
      setDeleteTarget(null);
      dispatch(fetchHomework({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed to delete');
    }
  };

  return (
    <Box>
      <PageHeader
        title="Homework"
        subtitle={`Total ${homework.totalCount || 0} assignments`}
        actions={
          isAdmin ? (
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
        onEdit={isAdmin ? (row) => navigate(`/homework/${row.id}/edit`) : undefined}
        onDelete={isAdmin ? (row) => setDeleteTarget(row) : undefined}
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
    </Box>
  );
}
