import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchEmployees, deleteEmployee } from '../../store/slices/employeeSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

export default function EmployeeListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { employees, loading } = useSelector((state) => state.employees);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin');

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);

  useEffect(() => {
    dispatch(
      fetchEmployees({
        page: page + 1,
        pageSize: rowsPerPage,
      })
    );
  }, [dispatch, page, rowsPerPage]);

  const columns = [
    { id: 'employeeCode', header: 'Employee ID', accessor: 'employeeCode', minWidth: 120 },
    { id: 'name', header: 'Name', accessor: (row) => `${row.firstName || ''} ${row.lastName || ''}`.trim(), minWidth: 180 },
    { id: 'email', header: 'Email', accessor: 'email', minWidth: 200 },
    { id: 'department', header: 'Department', accessor: 'departmentName', minWidth: 140 },
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
    navigate(`/employees/${row.id}/edit`);
  };

  const handleDelete = (row) => {
    setDeleteTarget(row);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteEmployee(deleteTarget.id));
    if (deleteEmployee.fulfilled.match(result)) {
      toast.success('Employee deleted successfully');
      setDeleteTarget(null);
      dispatch(fetchEmployees({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed to delete employee');
    }
  };

  return (
    <Box>
      <PageHeader
        title="Employees"
        subtitle={`Total ${employees.totalCount || 0} employees`}
        actions={
          isAdmin ? (
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={() => navigate('/employees/create')}
            >
              Add Employee
            </Button>
          ) : null
        }
      />

      <DataTable
        columns={columns}
        rows={employees.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={employees.totalCount || 0}
        searchPlaceholder="Search employees..."
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onEdit={isAdmin ? handleEdit : undefined}
        onDelete={isAdmin ? handleDelete : undefined}
        emptyMessage="No employees found"
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Employee"
        message={`Are you sure you want to delete ${deleteTarget?.fullName || 'this employee'}? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  );
}
