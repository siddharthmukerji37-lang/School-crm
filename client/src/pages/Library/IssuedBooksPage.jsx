import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box,
  Button,
  Stack,
  Tabs,
  Tab,
  TextField,
  MenuItem,
  Chip,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import { fetchIssuedBooks, issueBook, returnBook, fetchBooks } from '../../store/slices/librarySlice';
import { fetchStudents } from '../../store/slices/studentSlice';
import { fetchTeachers } from '../../store/slices/teacherSlice';
import toast from 'react-hot-toast';

function formatDate(value) {
  if (!value) return '-';
  const d = new Date(value);
  return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

export default function IssuedBooksPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { issuedBooks, books, loading } = useSelector((state) => state.library);
  const { students } = useSelector((state) => state.students);
  const { teachers } = useSelector((state) => state.teachers);

  const [tab, setTab] = useState(0);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [returnTarget, setReturnTarget] = useState(null);
  const [form, setForm] = useState({ bookId: '', borrowerType: 'student', studentId: '', teacherId: '' });

  useEffect(() => {
    dispatch(fetchIssuedBooks({ page: page + 1, pageSize: rowsPerPage }));
  }, [dispatch, page, rowsPerPage]);

  useEffect(() => {
    dispatch(fetchBooks({ page: 1, pageSize: 200 }));
    dispatch(fetchStudents({ page: 1, pageSize: 500 }));
    dispatch(fetchTeachers({ page: 1, pageSize: 500 }));
  }, [dispatch]);

  const allIssues = issuedBooks.items || [];
  const studentIssues = allIssues.filter((r) => r.studentId);
  const teacherIssues = allIssues.filter((r) => r.teacherId);
  const currentIssues = tab === 0 ? studentIssues : teacherIssues;

  const openDialog = () => {
    setForm({ bookId: '', borrowerType: 'student', studentId: '', teacherId: '' });
    setDialogOpen(true);
  };

  const handleIssue = async () => {
    if (!form.bookId) {
      toast.error('Select a book');
      return;
    }
    if (form.borrowerType === 'teacher' && !form.teacherId) {
      toast.error('Select a teacher');
      return;
    }
    if (form.borrowerType === 'student' && !form.studentId) {
      toast.error('Select a student');
      return;
    }
    setSubmitting(true);
    try {
      const payload =
        form.borrowerType === 'teacher'
          ? { bookId: form.bookId, teacherId: form.teacherId }
          : { bookId: form.bookId, studentId: form.studentId };
      const result = await dispatch(issueBook(payload));
      if (issueBook.fulfilled.match(result)) {
        toast.success('Book issued');
        setDialogOpen(false);
        dispatch(fetchIssuedBooks({ page: page + 1, pageSize: rowsPerPage }));
      } else {
        toast.error(result.payload || 'Failed to issue book');
      }
    } finally {
      setSubmitting(false);
    }
  };

  const confirmReturn = async () => {
    if (!returnTarget) return;
    const result = await dispatch(returnBook(returnTarget.id));
    if (returnBook.fulfilled.match(result)) {
      toast.success('Book returned');
      setReturnTarget(null);
      dispatch(fetchIssuedBooks({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed');
    }
  };

  const columns = [
    { id: 'bookTitle', header: 'Book', accessor: 'bookTitle', minWidth: 180 },
    {
      id: 'borrower',
      header: 'Borrower',
      minWidth: 180,
      render: (value, row) => {
        if (row?.teacherId) {
          return (
            <Stack direction="row" spacing={1} alignItems="center">
              <Typography variant="body2">{row.teacherName || 'Teacher'}</Typography>
              <Chip label="Teacher" size="small" color="info" variant="outlined" />
            </Stack>
          );
        }
        return (
          <Stack direction="row" spacing={1} alignItems="center">
            <Typography variant="body2">{row?.studentName || 'Student'}</Typography>
            <Chip label="Student" size="small" color="primary" variant="outlined" />
          </Stack>
        );
      },
    },
    { id: 'issueDate', header: 'Issue Date', accessor: (row) => formatDate(row.issueDate), minWidth: 110 },
    { id: 'dueDate', header: 'Due Date', accessor: (row) => formatDate(row.dueDate), minWidth: 110 },
    {
      id: 'status', header: 'Status', minWidth: 110,
      accessor: (row) =>
        row.isReturned ? 'Returned' : row.dueDate && new Date(row.dueDate) < new Date() ? 'Overdue' : 'Issued',
      render: (v) => (
        <Chip
          label={v}
          color={v === 'Returned' ? 'success' : v === 'Overdue' ? 'error' : 'primary'}
          size="small"
          variant="outlined"
        />
      ),
    },
    { id: 'fineAmount', header: 'Fine', accessor: (row) => (row.fineAmount ? `$${row.fineAmount}` : '-'), minWidth: 90, align: 'center' },
  ];

  const isReturned = (row) => row.isReturned === true;

  return (
    <Box>
      <PageHeader
        title="Issued Books"
        subtitle={`${studentIssues.length} students • ${teacherIssues.length} teachers`}
        actions={
          <Stack direction="row" spacing={1}>
            <Button startIcon={<ArrowBackIcon />} variant="outlined" onClick={() => navigate('/library')}>
              Back to Library
            </Button>
            <Button variant="contained" startIcon={<AddIcon />} onClick={openDialog}>
              Issue Book
            </Button>
          </Stack>
        }
      />

      <Tabs
        value={tab}
        onChange={(_, v) => setTab(v)}
        sx={{ mb: 2, borderBottom: 1, borderColor: 'divider' }}
      >
        <Tab label={`Students (${studentIssues.length})`} />
        <Tab label={`Teachers (${teacherIssues.length})`} />
      </Tabs>

      <DataTable
        columns={columns}
        rows={currentIssues}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={currentIssues.length}
        searchPlaceholder="Search issued books..."
        onPageChange={(_, p) => setPage(p)}
        onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
        onReturn={(row) => (!isReturned(row) ? setReturnTarget(row) : undefined)}
        emptyMessage={tab === 0 ? 'No books issued to students yet' : 'No books issued to teachers yet'}
      />

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Issue Book</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <Tabs
              value={form.borrowerType}
              onChange={(_, v) => setForm({ ...form, borrowerType: v, studentId: '', teacherId: '' })}
              sx={{ borderBottom: 1, borderColor: 'divider' }}
            >
              <Tab label="Student" value="student" />
              <Tab label="Teacher" value="teacher" />
            </Tabs>
            <TextField
              select
              fullWidth
              label="Select Book"
              value={form.bookId}
              onChange={(e) => setForm({ ...form, bookId: e.target.value })}
            >
              {(books.items || [])
                .filter((b) => b.availableCopies > 0)
                .map((b) => (
                  <MenuItem key={b.id} value={b.id}>
                    {b.title} {b.author ? `- ${b.author}` : ''} (Available: {b.availableCopies})
                  </MenuItem>
                ))}
            </TextField>
            {form.borrowerType === 'student' ? (
              <TextField
                select
                fullWidth
                label="Select Student"
                value={form.studentId}
                onChange={(e) => setForm({ ...form, studentId: e.target.value })}
              >
                {(students?.items || []).map((s) => (
                  <MenuItem key={s.id} value={s.id}>
                    {s.firstName} {s.lastName} {s.admissionNumber ? `(${s.admissionNumber})` : ''}
                  </MenuItem>
                ))}
              </TextField>
            ) : (
              <TextField
                select
                fullWidth
                label="Select Teacher"
                value={form.teacherId}
                onChange={(e) => setForm({ ...form, teacherId: e.target.value })}
              >
                {(teachers?.items || []).map((t) => (
                  <MenuItem key={t.id} value={t.id}>
                    {t.firstName} {t.lastName} {t.employeeId ? `(${t.employeeId})` : ''}
                  </MenuItem>
                ))}
              </TextField>
            )}
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setDialogOpen(false)} variant="outlined">Cancel</Button>
          <Button onClick={handleIssue} variant="contained" disabled={submitting}>
            {submitting ? <CircularProgress size={20} /> : 'Issue Book'}
          </Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={!!returnTarget}
        title="Return Book"
        message={`Mark "${returnTarget?.bookTitle}" as returned?`}
        confirmText="Return"
        onConfirm={confirmReturn}
        onCancel={() => setReturnTarget(null)}
      />
    </Box>
  );
}
