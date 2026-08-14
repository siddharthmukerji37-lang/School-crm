import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Formik, Form } from 'formik';
import { Box, TextField, MenuItem, Button, Paper, Typography, CircularProgress, Divider, Stack } from '@mui/material';
import Grid from '@mui/material/Grid2';
import SaveIcon from '@mui/icons-material/Save';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import * as Yup from 'yup';
import { createBook, updateBook, fetchBookById, clearSelectedBook } from '../../store/slices/librarySlice';
import toast from 'react-hot-toast';

const CATEGORY_OPTIONS = ['Fiction', 'Non-Fiction', 'Science', 'Mathematics', 'History', 'Literature', 'Reference', 'Technology', 'Arts', 'Other'];

const bookSchema = Yup.object({
  title: Yup.string().trim().required('Title is required'),
  author: Yup.string().trim().required('Author is required'),
  isbn: Yup.string().trim().required('ISBN is required'),
  category: Yup.string().required('Category is required'),
  publisher: Yup.string().trim(),
  totalCopies: Yup.number().transform((v, o) => o === '' ? undefined : v).required('Total copies required').min(1),
  availableCopies: Yup.number().transform((v, o) => o === '' ? undefined : v).required('Available copies required').min(0),
  description: Yup.string().trim(),
  location: Yup.string().trim(),
});

export default function BookFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedBook, loading } = useSelector((state) => state.library);
  const isEditMode = Boolean(id);

  const [initialValues, setInitialValues] = useState({
    title: '', author: '', isbn: '', category: '', publisher: '',
    totalCopies: '', availableCopies: '', description: '', location: '',
  });

  useEffect(() => {
    if (isEditMode) dispatch(fetchBookById(id));
    return () => dispatch(clearSelectedBook());
  }, [dispatch, id, isEditMode]);

  useEffect(() => {
    if (isEditMode && selectedBook) {
      setInitialValues({
        title: selectedBook.title || '', author: selectedBook.author || '',
        isbn: selectedBook.isbn || '', category: selectedBook.category || '',
        publisher: selectedBook.publisher || '',
        totalCopies: selectedBook.totalCopies ?? '',
        availableCopies: selectedBook.availableCopies ?? '',
        description: selectedBook.description || '', location: selectedBook.location || '',
      });
    }
  }, [isEditMode, selectedBook]);

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      const payload = {
        ...values,
        totalCopies: Number(values.totalCopies),
        availableCopies: Number(values.availableCopies),
        shelfNumber: values.location || '',
      };
      const action = isEditMode
        ? await dispatch(updateBook({ id, data: payload }))
        : await dispatch(createBook(payload));
      const successAction = isEditMode ? updateBook : createBook;
      if (successAction.fulfilled.match(action)) {
        toast.success(isEditMode ? 'Book updated' : 'Book created');
        navigate('/library');
      } else {
        toast.error(action.payload || 'Failed');
      }
    } finally { setSubmitting(false); }
  };

  if (isEditMode && loading && !selectedBook) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/library')} variant="outlined">Back</Button>
        <Typography variant="h4" fontWeight={700}>{isEditMode ? 'Edit Book' : 'Add New Book'}</Typography>
      </Box>
      <Formik initialValues={initialValues} validationSchema={bookSchema} onSubmit={handleSubmit} enableReinitialize>
        {({ values, errors, touched, handleChange, handleBlur, isSubmitting }) => (
          <Form>
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Book Information</Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="title" label="Title" value={values.title}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.title && Boolean(errors.title)} helperText={touched.title && errors.title} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="author" label="Author" value={values.author}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.author && Boolean(errors.author)} helperText={touched.author && errors.author} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="isbn" label="ISBN" value={values.isbn}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.isbn && Boolean(errors.isbn)} helperText={touched.isbn && errors.isbn} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth select name="category" label="Category" value={values.category}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.category && Boolean(errors.category)} helperText={touched.category && errors.category}>
                    {CATEGORY_OPTIONS.map((o) => <MenuItem key={o} value={o}>{o}</MenuItem>)}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="publisher" label="Publisher" value={values.publisher}
                    onChange={handleChange} onBlur={handleBlur} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="description" label="Description" multiline rows={2}
                    value={values.description} onChange={handleChange} onBlur={handleBlur} />
                </Grid>
              </Grid>
            </Paper>
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Inventory Details</Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="totalCopies" label="Total Copies" type="number" value={values.totalCopies}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.totalCopies && Boolean(errors.totalCopies)} helperText={touched.totalCopies && errors.totalCopies} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="availableCopies" label="Available Copies" type="number" value={values.availableCopies}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.availableCopies && Boolean(errors.availableCopies)} helperText={touched.availableCopies && errors.availableCopies} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="location" label="Shelf Location" value={values.location}
                    onChange={handleChange} onBlur={handleBlur} />
                </Grid>
              </Grid>
            </Paper>
            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button variant="outlined" onClick={() => navigate('/library')}>Cancel</Button>
              <Button type="submit" variant="contained" startIcon={<SaveIcon />} disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : isEditMode ? 'Update Book' : 'Create Book'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
