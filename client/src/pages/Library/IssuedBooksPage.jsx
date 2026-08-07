import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box,
  Button,
  Stack,
  TextField,
  MenuItem,
  Chip,
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

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [returnTarget, setReturnTarget] = useState(null);
  const [form, setForm] = useState({ bookId: '', studentId: '' });

  useEffect(() => {
    dispatch(fetchIssuedBooks({ page: page + 1, pageSize: rowsPerPage }));
  }, [dispatch, page, rowsPerPage]);

  useEffect(() => {
    dispatch(fetchBooks({ page: 1, pageSize: 200 }));
    dispatch(fetchStudents({ page: 1, pageSize: 500 }));
  }, [dispatch]);

  const openDialog = () => {
    setForm({ bookId: '', studentId: '' });
    setDialogOpen(true);
  };

  const handleIssue = async () => {
    if (!form.bookId || !form.studentId) {
      toast.error('Select both a book and a student');
      return;
    }
    setSubmitting(true);
    try {
      const result = await dispatch(
        issueBook({ bookId: form.bookId, studentId: form.studentId })
      );
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
    { id: 'studentName', header: 'Student', accessor: 'studentName', minWidth: 160 },
    { id: 'issueDate', header: 'Issue Date', accessor: (row) => formatDate(row.issueDate), minWidth: 110 },
    { id: 'dueDate', header: 'Due Date', accessor: (row) => formatDate(row.dueDate), minWidth: 110 },
    {
      id: 'status', header: 'Status', accessor: 'status', minWidth: 110,
      render: (v) => (
        <Chip
          label={v === 'Returned' ? 'Returned' : v === 'Overdue' ? 'Overdue' : 'Issued'}
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
        subtitle={`Total ${issuedBooks.totalCount || 0} issues`}
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
      <DataTable
        columns={columns}
        rows={issuedBooks.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={issuedBooks.totalCount || 0}
        searchPlaceholder="Search issued books..."
        onPageChange={(_, p) => setPage(p)}
        onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
        onReturn={(row) => (!isReturned(row) ? setReturnTarget(row) : undefined)}
        emptyMessage="No books issued yet"
      />

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Issue Book</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
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
