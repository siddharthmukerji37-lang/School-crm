import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip, MenuItem, TextField, Stack } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchStudents, deleteStudent } from '../../store/slices/studentSlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import { hasAdminRole } from '../../utils/roles';
import toast from 'react-hot-toast';

const CLASS_OPTIONS = [
  { value: '', label: 'All Classes' },
  { value: '1', label: 'Class 1' },
  { value: '2', label: 'Class 2' },
  { value: '3', label: 'Class 3' },
  { value: '4', label: 'Class 4' },
  { value: '5', label: 'Class 5' },
  { value: '6', label: 'Class 6' },
  { value: '7', label: 'Class 7' },
  { value: '8', label: 'Class 8' },
  { value: '9', label: 'Class 9' },
  { value: '10', label: 'Class 10' },
  { value: '11', label: 'Class 11' },
  { value: '12', label: 'Class 12' },
];

export default function StudentListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { students, loading } = useSelector((state) => state.students);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = hasAdminRole(user?.roles);

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [classFilter, setClassFilter] = useState('');
  const [deleteTarget, setDeleteTarget] = useState(null);

  useEffect(() => {
    dispatch(
      fetchStudents({
        page: page + 1,
        pageSize: rowsPerPage,
        classId: classFilter || undefined,
      })
    );
  }, [dispatch, page, rowsPerPage, classFilter]);

  const columns = [
    { id: 'admissionNumber', header: 'Adm. No.', accessor: 'admissionNumber', minWidth: 100 },
    { id: 'name', header: 'Name', accessor: (row) => `${row.firstName || ''} ${row.lastName || ''}`.trim(), minWidth: 180 },
    { id: 'class', header: 'Class', accessor: 'className', minWidth: 100 },
    { id: 'section', header: 'Section', accessor: 'sectionName', minWidth: 80 },
    { id: 'parentName', header: 'Parent', accessor: 'parentName', minWidth: 150 },
    { id: 'email', header: 'Email', accessor: 'email', minWidth: 200 },
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

  const handleView = (row) => {
    navigate(`/students/${row.id}`);
  };

  const handleEdit = (row) => {
    navigate(`/students/${row.id}/edit`);
  };

  const handleDelete = (row) => {
    setDeleteTarget(row);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteStudent(deleteTarget.id));
    if (deleteStudent.fulfilled.match(result)) {
      toast.success('Student deleted successfully');
      setDeleteTarget(null);
      dispatch(
        fetchStudents({
          page: page + 1,
          pageSize: rowsPerPage,
          classId: classFilter || undefined,
        })
      );
    } else {
      toast.error(result.payload || 'Failed to delete student');
    }
  };

  return (
    <Box>
      <PageHeader
        title="Students"
        subtitle={`Total ${students.totalCount || 0} students`}
        actions={
          <Stack direction="row" spacing={2} alignItems="center">
            <TextField
              select
              size="small"
              label="Filter by Class"
              value={classFilter}
              onChange={(e) => {
                setClassFilter(e.target.value);
                setPage(0);
              }}
              sx={{ minWidth: 160 }}
            >
              {CLASS_OPTIONS.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </TextField>
            {isAdmin && (
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => navigate('/students/create')}
              >
                Add Student
              </Button>
            )}
          </Stack>
        }
      />

      <DataTable
        columns={columns}
        rows={students.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={students.totalCount || 0}
        searchPlaceholder="Search students..."
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onView={handleView}
        onEdit={isAdmin ? handleEdit : undefined}
        onDelete={isAdmin ? handleDelete : undefined}
        onRowClick={handleView}
        emptyMessage="No students found"
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Student"
        message={`Are you sure you want to delete ${deleteTarget?.fullName || 'this student'}? This action cannot be undone.`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  );
}
