import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box, Paper, Typography, Chip, Button, Stack, TextField, MenuItem,
  RadioGroup, FormControlLabel, Radio, Grid, CircularProgress, IconButton,
  Tooltip, Divider, Card, CardContent,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import UploadIcon from '@mui/icons-material/Upload';
import FactCheckIcon from '@mui/icons-material/FactCheck';
import ImageIcon from '@mui/icons-material/Image';
import toast from 'react-hot-toast';
import { fetchExamById, clearSelectedExam } from '../../store/slices/examSlice';
import examService from '../../services/examService';
import axiosInstance from '../../services/axiosInstance';
import { uploadFile } from '../../utils/upload';
import ConfirmDialog from '../../components/common/ConfirmDialog';

const approvalColor = (s) => {
  switch (s) {
    case 'Approved': return 'success';
    case 'Rejected': return 'error';
    default: return 'warning';
  }
};

const emptyQuestion = {
  questionText: '', questionType: 'MCQ', optionA: '', optionB: '', optionC: '',
  optionD: '', correctAnswer: '', marks: '', subjectId: '', imageUrl: '', imageFileName: '',
};

export default function ExamQuestionsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedExam, loading } = useSelector((state) => state.exams);
  const { user } = useSelector((state) => state.auth);
  const isAdmin = (user?.roles || []).some((r) => r === 'SuperAdmin' || r === 'Admin');

  const [questions, setQuestions] = useState([]);
  const [questionsLoading, setQuestionsLoading] = useState(true);
  const [subjects, setSubjects] = useState([]);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyQuestion);
  const [showForm, setShowForm] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [paperUploading, setPaperUploading] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const isFinal = selectedExam?.examType === 'Final';
  const canEdit = isAdmin || !isFinal;

  const loadQuestions = async () => {
    try {
      const res = await examService.getQuestions(id);
      setQuestions(res.data.data || []);
    } catch (e) {
      toast.error(e.message || 'Failed to load questions');
    } finally {
      setQuestionsLoading(false);
    }
  };

  useEffect(() => {
    dispatch(fetchExamById(id));
    loadQuestions();
    axiosInstance.get('/schools').then(async (r) => {
      const schools = r.data.data?.items || r.data.data || [];
      if (schools.length > 0) {
        const sub = await axiosInstance.get(`/schools/${schools[0].id}/subjects`);
        setSubjects(sub.data.data || []);
      }
    }).catch(() => {});
    return () => dispatch(clearSelectedExam());
  }, [id]);

  const handleAddQuestion = () => {
    setForm({ ...emptyQuestion, marks: '' });
    setEditing(null);
    setShowForm(true);
  };

  const handleEditQuestion = (q) => {
    setForm({
      questionText: q.questionText || '', questionType: q.questionType,
      optionA: q.optionA || '', optionB: q.optionB || '', optionC: q.optionC || '',
      optionD: q.optionD || '', correctAnswer: q.correctAnswer || '',
      marks: q.marks ?? '', subjectId: q.subjectId || '', imageUrl: q.imageUrl || '',
      imageFileName: q.imageFileName || '',
    });
    setEditing(q);
    setShowForm(true);
  };

  const handleImageUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      const res = await uploadFile(file);
      if (res.success) {
        setForm((f) => ({ ...f, imageUrl: res.url, imageFileName: res.fileName }));
        toast.success('Image uploaded');
      } else {
        toast.error(res.message || 'Upload failed');
      }
    } catch (err) {
      toast.error(err.message || 'Upload failed');
    }
  };

  const handleSaveQuestion = async () => {
    if (!form.questionText.trim() && !form.imageUrl) {
      toast.error('Question text or image is required');
      return;
    }
    if (form.questionType === 'MCQ') {
      if (!form.optionA.trim() || !form.optionB.trim() || !form.correctAnswer) {
        toast.error('MCQ requires at least options A, B and a correct answer');
        return;
      }
    }
    const payload = {
      questionText: form.questionText, questionType: form.questionType,
      optionA: form.optionA, optionB: form.optionB, optionC: form.optionC,
      optionD: form.optionD, correctAnswer: form.correctAnswer,
      marks: Number(form.marks) || 0, subjectId: form.subjectId || null,
      imageUrl: form.imageUrl || null, imageFileName: form.imageFileName || null,
    };
    setSubmitting(true);
    try {
      if (editing) {
        await examService.updateQuestion(id, editing.id, payload);
        toast.success('Question updated');
      } else {
        await examService.addQuestions(id, [payload]);
        toast.success('Question added');
      }
      setShowForm(false);
      loadQuestions();
    } catch (err) {
      toast.error(err.message || 'Failed to save question');
    } finally {
      setSubmitting(false);
    }
  };

  const confirmDelete = async () => {
    try {
      await examService.deleteQuestion(id, deleteTarget.id);
      toast.success('Question deleted');
      setDeleteTarget(null);
      loadQuestions();
    } catch (err) {
      toast.error(err.message || 'Failed to delete question');
    }
  };

  const handlePaperUpload = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setPaperUploading(true);
    try {
      const res = await uploadFile(file);
      if (!res.success) {
        toast.error(res.message || 'Upload failed');
        return;
      }
      await examService.uploadQuestionPaper(id, { fileUrl: res.url, fileName: res.fileName });
      toast.success('Question paper uploaded');
      dispatch(fetchExamById(id));
    } catch (err) {
      toast.error(err.message || 'Upload failed');
    } finally {
      setPaperUploading(false);
    }
  };

  if (loading || !selectedExam) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/exams')} variant="outlined">Back</Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>{selectedExam.name}</Typography>
          <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 1 }}>
            <Chip label={selectedExam.examType || 'Exam'} color="primary" size="small" />
            <Chip label={selectedExam.className} color="secondary" size="small" variant="outlined" />
            <Chip
              label={selectedExam.approvalStatus}
              color={approvalColor(selectedExam.approvalStatus)}
              size="small"
              icon={<CheckCircleIcon />}
            />
          </Stack>
        </Box>
        {canEdit && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleAddQuestion}>
            Add Question
          </Button>
        )}
      </Box>

      {selectedExam.rejectionReason && (
        <Paper sx={{ p: 2, mb: 3, bgcolor: 'error.light', color: 'error.contrastText' }}>
          <Typography variant="body2">
            <strong>Rejection reason:</strong> {selectedExam.rejectionReason}
          </Typography>
        </Paper>
      )}

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, md: 8 }}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>Question Paper</Typography>
            <Divider sx={{ mb: 2 }} />
            {questionsLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress size={28} /></Box>
            ) : questions.length === 0 ? (
              <Typography variant="body2" color="text.secondary" sx={{ py: 3 }}>
                No questions yet. Use "Add Question" to build the paper.
              </Typography>
            ) : (
              questions.map((q, idx) => (
                <Card key={q.id} sx={{ mb: 2 }}>
                  <CardContent sx={{ py: 2, '&:last-child': { pb: 2 } }}>
                    <Stack direction="row" spacing={1} alignItems="flex-start" justifyContent="space-between">
                      <Box sx={{ flex: 1 }}>
                        <Stack direction="row" spacing={1} sx={{ mb: 0.5 }}>
                          <Typography variant="body1" fontWeight={600}>
                            {idx + 1}. {q.questionText || '(Image based question)'}
                          </Typography>
                          <Chip label={q.questionType} size="small" color={q.questionType === 'MCQ' ? 'info' : 'warning'} variant="outlined" />
                          <Chip label={`${q.marks} marks`} size="small" variant="outlined" />
                        </Stack>
                        {q.imageUrl && (
                          <Box sx={{ my: 1 }}>
                            <a href={q.imageUrl} target="_blank" rel="noreferrer">
                              <Button size="small" startIcon={<ImageIcon />} component="span">View image</Button>
                            </a>
                          </Box>
                        )}
                        {q.questionType === 'MCQ' ? (
                          <Box sx={{ pl: 1 }}>
                            {[['A', q.optionA], ['B', q.optionB], ['C', q.optionC], ['D', q.optionD]].map(([opt, val]) =>
                              val ? (
                                <Typography key={opt} variant="body2" color={q.correctAnswer === opt ? 'success.main' : 'text.secondary'}>
                                  {opt}. {val}{q.correctAnswer === opt ? '  ✓' : ''}
                                </Typography>
                              ) : null
                            )}
                          </Box>
                        ) : null}
                      </Box>
                      {canEdit && (
                        <Stack direction="row">
                          <Tooltip title="Edit">
                            <IconButton size="small" onClick={() => handleEditQuestion(q)}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton size="small" color="error" onClick={() => setDeleteTarget(q)}>
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Stack>
                      )}
                    </Stack>
                  </CardContent>
                </Card>
              ))
            )}
          </Paper>

          {showForm && (
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                {editing ? 'Edit Question' : 'Add Question'}
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField
                    fullWidth select label="Question Type" size="small" value={form.questionType}
                    onChange={(e) => setForm({ ...form, questionType: e.target.value })}
                  >
                    <MenuItem value="MCQ">Multiple Choice</MenuItem>
                    <MenuItem value="Descriptive">Descriptive</MenuItem>
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField
                    fullWidth label="Marks" type="number" size="small" value={form.marks}
                    onChange={(e) => setForm({ ...form, marks: e.target.value })}
                  />
                </Grid>
                <Grid size={{ xs: 12, sm: 4 }}>
                  <TextField
                    fullWidth select label="Subject (optional)" size="small" value={form.subjectId}
                    onChange={(e) => setForm({ ...form, subjectId: e.target.value })}
                  >
                    <MenuItem value=""><em>None</em></MenuItem>
                    {subjects.map((s) => <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>)}
                  </TextField>
                </Grid>
                <Grid size={{ xs: 12 }}>
                  <TextField
                    fullWidth label="Question Text" size="small" multiline rows={2} value={form.questionText}
                    onChange={(e) => setForm({ ...form, questionText: e.target.value })}
                  />
                </Grid>

                {form.questionType === 'MCQ' ? (
                  <>
                    {['A', 'B', 'C', 'D'].map((opt) => (
                      <Grid key={opt} size={{ xs: 12, sm: 6 }}>
                        <TextField
                          fullWidth label={`Option ${opt}`} size="small" value={form[`option${opt}`]}
                          onChange={(e) => setForm({ ...form, [`option${opt}`]: e.target.value })}
                        />
                      </Grid>
                    ))}
                    <Grid size={{ xs: 12 }}>
                      <RadioGroup row value={form.correctAnswer}
                        onChange={(e) => setForm({ ...form, correctAnswer: e.target.value })}>
                        {['A', 'B', 'C', 'D'].map((opt) => (
                          <FormControlLabel key={opt} value={opt} control={<Radio size="small" />}
                            label={`Correct: ${opt}`} />
                        ))}
                      </RadioGroup>
                    </Grid>
                  </>
                ) : (
                  <Grid size={{ xs: 12 }}>
                    <Box>
                      <Button
                        variant="outlined" component="label" size="small" startIcon={<UploadIcon />}
                        sx={{ mb: 1 }}
                      >
                        Upload Question Image
                        <input type="file" hidden accept="image/*" onChange={handleImageUpload} />
                      </Button>
                      {form.imageUrl && (
                        <Typography variant="body2" color="success.main">
                          Image attached: {form.imageFileName || form.imageUrl}
                        </Typography>
                      )}
                    </Box>
                  </Grid>
                )}
              </Grid>
              <Stack direction="row" spacing={2} justifyContent="flex-end" sx={{ mt: 3 }}>
                <Button variant="outlined" onClick={() => setShowForm(false)}>Cancel</Button>
                <Button variant="contained" disabled={submitting} onClick={handleSaveQuestion}>
                  {submitting ? 'Saving...' : editing ? 'Update Question' : 'Save Question'}
                </Button>
              </Stack>
            </Paper>
          )}
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          {canEdit && (
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" fontWeight={600} gutterBottom>Question Paper File</Typography>
              <Divider sx={{ mb: 2 }} />
              {selectedExam.questionPaperUrl ? (
                <Box>
                  <Typography variant="body2" sx={{ mb: 1 }}>
                    Uploaded: {selectedExam.questionPaperFileName || selectedExam.questionPaperUrl}
                  </Typography>
                  <Button size="small" href={selectedExam.questionPaperUrl} target="_blank" variant="outlined">
                    View Paper
                  </Button>
                </Box>
              ) : (
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  No paper file uploaded yet. Teachers can upload a printable paper (image or Word/PDF) for the exam.
                </Typography>
              )}
              <Button
                variant="outlined" component="label" size="small" startIcon={<UploadIcon />}
                disabled={paperUploading} sx={{ mt: 1 }}
              >
                {paperUploading ? 'Uploading...' : 'Upload / Replace Paper'}
                <input type="file" hidden accept=".jpg,.jpeg,.png,.webp,.pdf,.doc,.docx" onChange={handlePaperUpload} />
              </Button>
            </Paper>
          )}

          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" fontWeight={600} gutterBottom>Submissions</Typography>
            <Divider sx={{ mb: 2 }} />
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              View and grade student submissions for this exam.
            </Typography>
            <Button
              variant="contained" startIcon={<FactCheckIcon />} fullWidth
              onClick={() => navigate(`/exams/${id}/submissions`)}
            >
              Manage Submissions
            </Button>
          </Paper>
        </Grid>
      </Grid>

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Question"
        message={`Delete this question?`}
        confirmText="Delete"
        onConfirm={confirmDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  );
}
