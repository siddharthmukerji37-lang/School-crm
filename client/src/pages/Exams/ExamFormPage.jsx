import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Formik, Form } from 'formik';
import { Box, TextField, MenuItem, Button, Paper, Typography, CircularProgress, Divider, Stack } from '@mui/material';
import Grid from '@mui/material/Grid2';
import SaveIcon from '@mui/icons-material/Save';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import * as Yup from 'yup';
import { createExam, updateExam, fetchExamById, clearSelectedExam } from '../../store/slices/examSlice';
import axiosInstance from '../../services/axiosInstance';
import toast from 'react-hot-toast';

const ALL_EXAM_TYPES = ['Midterm', 'Final', 'Quiz', 'Assignment', 'Practical'];

const examSchema = Yup.object({
  name: Yup.string().trim().required('Exam name is required'),
  examType: Yup.string().required('Exam type is required'),
  classRoomId: Yup.string().required('Class is required'),
  sectionId: Yup.string(),
  startDate: Yup.date().nullable().required('Start date is required'),
  endDate: Yup.date().nullable().required('End date is required'),
  maxMarks: Yup.number().transform((v, o) => o === '' ? undefined : v).required('Max marks required').min(1),
  passingMarks: Yup.number().transform((v, o) => o === '' ? undefined : v).required('Passing marks required').min(0),
  description: Yup.string().trim(),
});

export default function ExamFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedExam, loading } = useSelector((state) => state.exams);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin');
  const examTypeOptions = isAdmin ? ALL_EXAM_TYPES : ALL_EXAM_TYPES.filter((t) => t !== 'Final');
  const isEditMode = Boolean(id);

  const [classes, setClasses] = useState([]);
  const [sections, setSections] = useState([]);
  const [initialValues, setInitialValues] = useState({
    name: '', examType: '', classRoomId: '', sectionId: '', startDate: '', endDate: '',
    maxMarks: '', passingMarks: '', description: '',
  });

  const fetchSections = async (classRoomId) => {
    if (!classRoomId) { setSections([]); return; }
    try {
      const res = await axiosInstance.get(`/schools/classes/${classRoomId}/sections`);
      setSections(res.data.data || []);
    } catch { setSections([]); }
  };

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
    if (isEditMode) dispatch(fetchExamById(id));
    return () => dispatch(clearSelectedExam());
  }, [dispatch, id, isEditMode]);

  useEffect(() => {
    if (isEditMode && selectedExam) {
      if (selectedExam.classRoomId) fetchSections(selectedExam.classRoomId);
      setInitialValues({
        name: selectedExam.name || '',
        examType: selectedExam.examType || '',
        classRoomId: selectedExam.classRoomId || '',
        sectionId: selectedExam.sectionId || '',
        startDate: selectedExam.startDate ? new Date(selectedExam.startDate).toISOString().split('T')[0] : '',
        endDate: selectedExam.endDate ? new Date(selectedExam.endDate).toISOString().split('T')[0] : '',
        maxMarks: selectedExam.maxMarks ?? '',
        passingMarks: selectedExam.passingMarks ?? '',
        description: selectedExam.description || '',
      });
    }
  }, [isEditMode, selectedExam]);

  const handleSubmit = async (values, { setSubmitting }) => {
    try {
      const payload = {
        ...values,
        sectionId: values.sectionId || null,
        maxMarks: Number(values.maxMarks),
        passingMarks: Number(values.passingMarks),
      };
      const action = isEditMode
        ? await dispatch(updateExam({ id, data: payload }))
        : await dispatch(createExam(payload));
      const successAction = isEditMode ? updateExam : createExam;
      if (successAction.fulfilled.match(action)) {
        toast.success(isEditMode ? 'Exam updated' : 'Exam created');
        const examId = isEditMode ? id : action.payload?.data?.id;
        if (!isAdmin) {
          navigate(examId ? `/exams/${examId}/questions` : '/exams');
        } else {
          navigate('/exams');
        }
      } else {
        toast.error(action.payload || 'Failed');
      }
    } finally { setSubmitting(false); }
  };

  if (isEditMode && loading && !selectedExam) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/exams')} variant="outlined">Back</Button>
        <Typography variant="h4" fontWeight={700}>{isEditMode ? 'Edit Exam' : 'Add New Exam'}</Typography>
      </Box>
      <Formik initialValues={initialValues} validationSchema={examSchema} onSubmit={handleSubmit} enableReinitialize>
        {({ values, errors, touched, handleChange, handleBlur, setFieldValue, isSubmitting }) => (
          <Form>
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Exam Details</Typography>
              <Divider sx={{ mb: 3 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="name" label="Exam Name" value={values.name}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.name && Boolean(errors.name)} helperText={touched.name && errors.name} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth select name="examType" label="Exam Type" value={values.examType}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.examType && Boolean(errors.examType)} helperText={touched.examType && errors.examType}>
                    {examTypeOptions.map((o) => <MenuItem key={o} value={o}>{o}</MenuItem>)}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth select name="classRoomId" label="Class" value={values.classRoomId}
                    onChange={(e) => { handleChange(e); setFieldValue('sectionId', ''); fetchSections(e.target.value); }}
                    onBlur={handleBlur}
                    error={touched.classRoomId && Boolean(errors.classRoomId)} helperText={touched.classRoomId && errors.classRoomId}>
                    {classes.map((c) => <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>)}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth select name="sectionId" label="Section" value={values.sectionId}
                    onChange={handleChange} onBlur={handleBlur} disabled={!values.classRoomId}
                    helperText={values.sectionId ? '' : 'Leave blank to include all sections'}
                    error={touched.sectionId && Boolean(errors.sectionId)}>
                    <MenuItem value=""><em>All Sections</em></MenuItem>
                    {sections.map((s) => <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>)}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="startDate" label="Start Date" type="date" value={values.startDate}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.startDate && Boolean(errors.startDate)} helperText={touched.startDate && errors.startDate}
                    slotProps={{ inputLabel: { shrink: true } }} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="endDate" label="End Date" type="date" value={values.endDate}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.endDate && Boolean(errors.endDate)} helperText={touched.endDate && errors.endDate}
                    slotProps={{ inputLabel: { shrink: true } }} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="maxMarks" label="Max Marks" type="number" value={values.maxMarks}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.maxMarks && Boolean(errors.maxMarks)} helperText={touched.maxMarks && errors.maxMarks} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="passingMarks" label="Passing Marks" type="number" value={values.passingMarks}
                    onChange={handleChange} onBlur={handleBlur}
                    error={touched.passingMarks && Boolean(errors.passingMarks)} helperText={touched.passingMarks && errors.passingMarks} />
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField fullWidth name="description" label="Description" multiline rows={3}
                    value={values.description} onChange={handleChange} onBlur={handleBlur} />
                </Grid>
              </Grid>
            </Paper>
            <Stack direction="row" spacing={2} justifyContent="flex-end">
              <Button variant="outlined" onClick={() => navigate('/exams')}>Cancel</Button>
              <Button type="submit" variant="contained" startIcon={<SaveIcon />} disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : isEditMode ? 'Update Exam' : 'Create Exam'}
              </Button>
            </Stack>
          </Form>
        )}
      </Formik>
    </Box>
  );
}
