import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Paper, Typography, Chip, Button, Divider, CircularProgress } from '@mui/material';
import Grid from '@mui/material/Grid2';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { fetchBookById, clearSelectedBook } from '../../store/slices/librarySlice';

function DetailRow({ label, value }) {
  return (
    <Box sx={{ display: 'flex', py: 1 }}>
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 180, fontWeight: 500 }}>
        {label}
      </Typography>
      <Typography variant="body2">{value || '-'}</Typography>
    </Box>
  );
}

export default function BookDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedBook, loading } = useSelector((state) => state.library);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin');

  useEffect(() => {
    dispatch(fetchBookById(id));
    return () => {
      dispatch(clearSelectedBook());
    };
  }, [dispatch, id]);

  if (loading || !selectedBook) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const book = selectedBook;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/library')} variant="outlined">
          Back
        </Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>
            Book Details
          </Typography>
        </Box>
        {isAdmin && (
          <Button
            variant="contained"
            startIcon={<EditIcon />}
            onClick={() => navigate(`/library/${id}/edit`)}
          >
            Edit
          </Button>
        )}
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h5" fontWeight={600}>
          {book.title}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          {book.author || 'Unknown Author'}
        </Typography>
        <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
          <Chip label={book.category || 'No Category'} color="primary" size="small" />
          <Chip
            label={book.isActive ? 'Active' : 'Inactive'}
            color={book.isActive ? 'success' : 'default'}
            size="small"
            variant="outlined"
          />
        </Box>
      </Paper>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
          Book Information
        </Typography>
        <Divider sx={{ mb: 1 }} />
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Title" value={book.title} />
            <DetailRow label="Author" value={book.author} />
            <DetailRow label="ISBN" value={book.isbn} />
            <DetailRow label="Publisher" value={book.publisher} />
            <DetailRow label="Category" value={book.category} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Total Copies" value={book.totalCopies} />
            <DetailRow label="Available Copies" value={book.availableCopies} />
            <DetailRow label="Shelf Location" value={book.shelfNumber} />
            <DetailRow label="Description" value={book.description} />
          </Grid>
        </Grid>
      </Paper>
    </Box>
  );
}
