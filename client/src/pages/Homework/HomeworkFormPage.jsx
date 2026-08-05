import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Formik, Form } from 'formik';
import {
  Box, Grid, TextField, MenuItem, Button, Paper, Typography,
  CircularProgress, Divider, Stack,
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import * as Yup from 'yup';
import { createHomework, updateHomework, fetchHomeworkById, clearSelectedHomework } from '../../store/slices/homeworkSlice';
import axiosInstance from '../../services/axiosInstance';
import toast from 'react-hot-toast';

const hwSchema = Yup.object({
  title: Yup.string().trim().required('Title is required'),
  description: Yup.string().trim().required('Description is required'),
  classRoomId: Yup.string().required('Class is required'),
  sectionId: Yup.string().required('Section is required'),
  subjectId: Yup.string().required('Subject is required'),
  dueDate: Yup.date().nullable().required('Due date is required'),
  assignedBy: Yup.string().trim().required('Assigned by is required'),
});

export default function HomeworkFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedHomework, loading } = useSelector((state) => state.homework);
  const isEditMode = Boolean(id);

  const [classes, setClasses] = useState([]);
  const [sections, setSections] = useState([]);
  const [subjects, setSubjects] = useState([]);
  const [initialValues, setInitialValues] = useState({
    title: '', description: '', classRoomId: '', sectionId: '', subjectId: '',
    dueDate: '', assignedBy: '',
  });

  useEffect(() => {
    const fetchClasses = async () => {
      try {
        const res = await axiosInstance.get('/schools');
        const schools = res.data.data?.items || res.data.data || [];
        if (schools.length > 0) {
          const classRes = await axiosInstance.get(`/schools/${schools[0].id}/classes`);
          setClasses(classRes.data.data || []);
        }
      } catch { setClasses([]); }
    };
    fetchClasses();
  }, []);

  useEffect(() => {
    if (!selectedHomework) return;
    const fetchSectionsAndSubjects = async () => {
      if (selectedHomework.classRoomId) {
        try {
          const secRes = await axiosInstance.get(`/schools/classes/${selectedHomework.classRoomId}/sections`);
          setSections(secRes.data.data || []);
        } catch { setSections([]); }
      }
    };
    fetchSectionsAndSubjects();
  }, [selectedHomework]);

  const fetchSections = async (classRoomId) => {
    if (!classRoomId) { setSections([]); return; }
    try {
      const res = await axiosInstance.get(`/schools/classes/${classRoomId}/sections`);
      setSections(res.data.data || []);
    } catch { setSections([]); }
  };

  useEffect(() => {
    if (isEditMode) dispatch(fetchHomeworkById(id));
    return () => dispatch(clearSelectedHomework());
  }, [dispatch, id, isEditMode]);

  useEffect(() => {
    if (isEditMode && selectedHomework) {
      setInitialValues({
        title: selectedHomework.title || '',
        description: selectedHomework.description || '',
        classRoomId: selectedHomework.classRoomId || '',
        sectionId: selectedHomework.sectionId || '',
        subjectId: selectedHomework.subjectId || '',
        dueDate: selectedHomework.dueDate ? new Date(selectedHomework.dueDate).toISOString().split('T')[0] : '',
        assignedBy: selectedHomework.assignedBy || '',
      });
    }
  }, [isEditMode, selectedHomework]);

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      const action = isEditMode
        ? await dispatch(updateHomework({ id, data: values }))
        : await dispatch(createHomework(values));
      const successAction = isEditMode ? updateHomework : createHomework;
      if (successAction.fulfilled.match(action)) {
        toast.success(isEditMode ? 'Homework updated' : 'Homework created');
        navigate('/homework');
      } else {
        toast.error(action.payload || 'Failed');
      }
    } finally { setSubmitting(false); }
  };

  if (isEditMode && loading && !selectedHomework) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/homework')} variant="outlined">Back</Button>
        <Typography variant="h4" fontWeight={700}>{isEditMode ? 'Edit Homework' : 'Add New Homework'}</Typography>
      </Box>
      <Formik initialValues={initialValues} validationSchema={hwSchema} onSubmit={handleSubmit} enableReinitialize>
        {({ values, errors, touched, handleChange, handleBlur, setFieldValue, isSubmitting }) => (
          <Form>
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Homework Details</Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="title" label="Title" value={values.title}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.title && Boolean(errors.title)} helperText={touched.title && errors.title} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="description" label="Description" multiline rows={3}
                    value={values.description} onChange={handleChange} onBlur={handleBlur}
                    error={touched.description && Boolean(errors.description)} helperText={touched.description && errors.description} />
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField fullWidth select name="classRoomId" label="Class" value={values.classRoomId}
                    onChange={(e) => { handleChange(e); setFieldValue('sectionId', ''); fetchSections(e.target.value); }}
                    onBlur={handleBlur}
                    error={touched.classRoomId && Boolean(errors.classRoomId)} helperText={touched.classRoomId && errors.classRoomId}>
                    {classes.map((c) => <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>)}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField fullWidth select name="sectionId" label="Section" value={values.sectionId}
                    onChange={handleChange} onBlur={handleBlur} disabled={!values.classRoomId}
                    error={touched.sectionId && Boolean(errors.sectionId)} helperText={touched.sectionId && errors.sectionId}>
                    {sections.map((s) => <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>)}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField fullWidth name="subjectId" label="Subject ID" value={values.subjectId}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.subjectId && Boolean(errors.subjectId)} helperText={touched.subjectId && errors.subjectId} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField fullWidth name="dueDate" label="Due Date" type="date" value={values.dueDate}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.dueDate && Boolean(errors.dueDate)} helperText={touched.dueDate && errors.dueDate}
                    slotProps={{ inputLabel: { shrink: true } }} />
                </Grid>
                <Grid size={{ xs: 12, sm: 6 }}>
                  <TextField fullWidth name="assignedBy" label="Assigned By" value={values.assignedBy}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.assignedBy && Boolean(errors.assignedBy)} helperText={touched.assignedBy && errors.assignedBy} />
                </Grid>
              </Grid>
            </Paper>
            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button variant="outlined" onClick={() => navigate('/homework')}>Cancel</Button>
              <Button type="submit" variant="contained" startIcon={<SaveIcon />} disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : isEditMode ? 'Update' : 'Create'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
