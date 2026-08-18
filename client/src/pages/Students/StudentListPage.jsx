import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip, MenuItem, TextField, Stack, CircularProgress } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchStudents, deleteStudent } from '../../store/slices/studentSlice';
import attendanceService from '../../services/attendanceService';
import axiosInstance from '../../services/axiosInstance';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import { hasAdminRole } from '../../utils/roles';
import toast from 'react-hot-toast';

const getTodayStr = () => {
  const now = new Date();
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
};

const attendanceChip = (status) => {
  if (!status) {
    return <Chip label="Not Marked" size="small" variant="outlined" color="default" />;
  }
  const color = status === 'Present' ? 'success' : status === 'Absent' ? 'error' : status === 'Late' ? 'warning' : 'default';
  return <Chip label={status} color={color} size="small" />;
};

export default function StudentListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { students, loading } = useSelector((state) => state.students);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = hasAdminRole(user?.roles);

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [classFilter, setClassFilter] = useState('');
  const [classes, setClasses] = useState([]);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [attendanceMap, setAttendanceMap] = useState({});

  useEffect(() => {
    const fetchClasses = async () => {
      try {
        const schoolsRes = await axiosInstance.get('/schools', { params: { pageSize: 1 } });
        const schoolId = schoolsRes.data?.data?.items?.[0]?.id;
        if (!schoolId) return;
        const classRes = await axiosInstance.get(`/schools/${schoolId}/classes`);
        setClasses(classRes.data?.data || []);
      } catch {
        setClasses([]);
      }
    };
    fetchClasses();
  }, []);

  useEffect(() => {
    dispatch(
      fetchStudents({
        page: page + 1,
        pageSize: rowsPerPage,
        classRoomId: classFilter || undefined,
      })
    );
  }, [dispatch, page, rowsPerPage, classFilter]);

  useEffect(() => {
    let cancelled = false;
    const loadAttendance = async () => {
      try {
        const res = await attendanceService.getAll({ date: getTodayStr(), pageSize: 1000 });
        const items = res.data.data?.items || [];
        const map = {};
        items.forEach((r) => {
          if (r.studentId) map[r.studentId] = r.status;
        });
        if (!cancelled) setAttendanceMap(map);
      } catch {
        if (!cancelled) setAttendanceMap({});
      }
    };
    loadAttendance();
    return () => { cancelled = true; };
  }, []);

  const columns = [
    { id: 'admissionNumber', header: 'Adm. No.', accessor: 'admissionNumber', minWidth: 100 },
    { id: 'name', header: 'Name', accessor: (row) => `${row.firstName || ''} ${row.lastName || ''}`.trim(), minWidth: 180 },
    { id: 'class', header: 'Class', accessor: 'className', minWidth: 100 },
    { id: 'section', header: 'Section', accessor: 'sectionName', minWidth: 80 },
    { id: 'parentName', header: 'Parent', accessor: 'parentName', minWidth: 150 },
    { id: 'email', header: 'Email', accessor: 'email', minWidth: 200 },
    { id: 'phone', header: 'Phone', accessor: 'phone', minWidth: 120 },
    {
      id: 'attendanceToday',
      header: 'Attendance Today',
      accessor: 'id',
      minWidth: 140,
      sortable: false,
      render: (value) => attendanceChip(attendanceMap[value]),
    },
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
          classRoomId: classFilter || undefined,
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
          <Stack direction="column" spacing={2} alignItems="stretch" sx={{ minWidth: 200 }}>
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
              <MenuItem value="">All Classes</MenuItem>
              {classes.map((cls) => (
                <MenuItem key={cls.id} value={cls.id}>
                  {cls.name}
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
