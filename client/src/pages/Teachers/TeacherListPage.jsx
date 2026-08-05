import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip, MenuItem, TextField, Stack } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchTeachers, deleteTeacher } from '../../store/slices/teacherSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

export default function TeacherListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { teachers, loading } = useSelector((state) => state.teachers);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin');

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);

  useEffect(() => {
    dispatch(
      fetchTeachers({
        page: page + 1,
        pageSize: rowsPerPage,
      })
    );
  }, [dispatch, page, rowsPerPage]);

  const columns = [
    { id: 'employeeId', header: 'Employee ID', accessor: 'employeeId', minWidth: 120 },
    { id: 'name', header: 'Name', accessor: 'firstName', minWidth: 180, render: (value, row) => `${value} ${row.lastName || ''}` },
    { id: 'email', header: 'Email', accessor: 'email', minWidth: 200 },
    { id: 'department', header: 'Department', accessor: 'departmentName', minWidth: 140, render: (value) => value || '—' },
    { id: 'phone', header: 'Phone', accessor: 'phone', minWidth: 120 },
    {
      id: 'status',
      header: 'Status',
      accessor: 'status',
      minWidth: 100,
      render: (value) => (
        <Chip
          label={value || 'Active'}
          color={value === 'Inactive' ? 'default' : 'success'}
          size="small"
          variant="outlined"
        />
      ),
    },
  ];

  const handlePageChange = (_, newPage) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  const handleEdit = (row) => {
    navigate(`/teachers/${row.id}/edit`);
  };

  const handleDelete = (row) => {
    setDeleteTarget(row);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteTeacher(deleteTarget.id));
    if (deleteTeacher.fulfilled.match(result)) {
      toast.success('Teacher deleted successfully');
      setDeleteTarget(null);
      dispatch(fetchTeachers({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed to delete teacher');
    }
  };

  return (
    <Box>
      <PageHeader
        title="Teachers"
        subtitle={`Total ${teachers.totalCount || 0} teachers`}
        actions={
          isAdmin ? (
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => navigate('/teachers/create')}
            >
              Add Teacher
            </Button>
          ) : null
        }
      />

      <DataTable
        columns={columns}
        rows={teachers.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={teachers.totalCount || 0}
        searchPlaceholder="Search teachers..."
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onEdit={isAdmin ? handleEdit : undefined}
        onDelete={isAdmin ? handleDelete : undefined}
        emptyMessage="No teachers found"
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Teacher"
        message={`Are you sure you want to delete ${deleteTarget?.firstName ? `${deleteTarget.firstName} ${deleteTarget.lastName || ''}` : 'this teacher'}? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  );
}
