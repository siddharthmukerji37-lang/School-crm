import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Button, Chip, TextField, MenuItem, Stack } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { fetchBooks, deleteBook } from '../../store/slices/librarySlice';
import PageHeader from '../../components/common/PageHeader';
import DataTable from '../../components/common/DataTable';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import toast from 'react-hot-toast';

export default function BookListPage() {
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { books, loading } = useSelector((state) => state.library);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin');

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [deleteTarget, setDeleteTarget] = useState(null);

  useEffect(() => {
    dispatch(fetchBooks({ page: page + 1, pageSize: rowsPerPage }));
  }, [dispatch, page, rowsPerPage]);

  const columns = [
    { id: 'title', header: 'Title', accessor: 'title', minWidth: 200 },
    { id: 'author', header: 'Author', accessor: 'author', minWidth: 150 },
    { id: 'isbn', header: 'ISBN', accessor: 'isbn', minWidth: 130 },
    { id: 'category', header: 'Category', accessor: 'category', minWidth: 120 },
    { id: 'totalCopies', header: 'Total', accessor: 'totalCopies', minWidth: 70, align: 'center' },
    { id: 'availableCopies', header: 'Available', accessor: 'availableCopies', minWidth: 90, align: 'center' },
    {
      id: 'status', header: 'Status', accessor: 'status', minWidth: 100,
      render: (v) => <Chip label={v || 'Available'} color={v === 'Unavailable' ? 'error' : 'success'} size="small" variant="outlined" />,
    },
  ];

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    const result = await dispatch(deleteBook(deleteTarget.id));
    if (deleteBook.fulfilled.match(result)) {
      toast.success('Book deleted');
      setDeleteTarget(null);
      dispatch(fetchBooks({ page: page + 1, pageSize: rowsPerPage }));
    } else {
      toast.error(result.payload || 'Failed');
    }
  };

  return (
    <Box>
      <PageHeader
        title="Library"
        subtitle={`Total ${books.totalCount || 0} books`}
        actions={
          isAdmin ? (
            <Stack direction="row" spacing={1}>
              <Button variant="outlined" onClick={() => navigate('/library/issued')}>Issued Books</Button>
              <Button variant="contained" startIcon={<AddIcon />} onClick={() => navigate('/library/create')}>
                Add Book
              </Button>
            </Stack>
          ) : null
        }
      />
      <DataTable
        columns={columns}
        rows={books.items || []}
        loading={loading}
        page={page}
        rowsPerPage={rowsPerPage}
        totalCount={books.totalCount || 0}
        searchPlaceholder="Search books..."
        onPageChange={(_, p) => setPage(p)}
        onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value, 10)); setPage(0); }}
        onEdit={isAdmin ? (row) => navigate(`/library/${row.id}/edit`) : undefined}
        onDelete={isAdmin ? (row) => setDeleteTarget(row) : undefined}
        emptyMessage="No books found"
      />
      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Book"
        message={`Delete "${deleteTarget?.title}"?`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  );
}
