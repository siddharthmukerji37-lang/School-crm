import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Box, Paper, Typography, Chip, Button, Stack, TextField, RadioGroup,
  FormControlLabel, Radio, CircularProgress, Divider, Card, CardContent,
  Alert, LinearProgress,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import SendIcon from '@mui/icons-material/Send';
import UploadIcon from '@mui/icons-material/Upload';
import toast from 'react-hot-toast';
import examService from '../../services/examService';
import { uploadFile } from '../../utils/upload';
import ConfirmDialog from '../../components/common/ConfirmDialog';

export default function StudentExamTakePage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [exam, setExam] = useState(null);
  const [questions, setQuestions] = useState([]);
  const [answers, setAnswers] = useState({});
  const [submission, setSubmission] = useState(null);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const [examRes, qRes, myRes] = await Promise.all([
        examService.getById(id),
        examService.getQuestions(id),
        examService.getMySubmissions(),
      ]);
      setExam(examRes.data.data);
      setQuestions(qRes.data.data || []);
      const mine = (myRes.data.data || []).find((s) => s.examId === id);
      setSubmission(mine || null);
    } catch (e) {
      toast.error(e.message || 'Failed to load exam');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [id]);

  const handleImageAnswer = async (questionId, e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      const res = await uploadFile(file);
      if (res.success) {
        setAnswers((a) => ({ ...a, [questionId]: { ...(a[questionId] || {}), imageUrl: res.url } }));
        toast.success('Answer image uploaded');
      } else {
        toast.error(res.message || 'Upload failed');
      }
    } catch (err) {
      toast.error(err.message || 'Upload failed');
    }
  };

  const handleSubmit = async () => {
    setSubmitting(true);
    try {
      const payload = {
        examId: id,
        answers: questions.map((q) => {
          const a = answers[q.id] || {};
          if (q.questionType === 'MCQ') return { examQuestionId: q.id, selectedOption: a.selectedOption || null };
          return {
            examQuestionId: q.id,
            answerText: a.answerText || null,
            imageUrl: a.imageUrl || null,
          };
        }),
      };
      const res = await examService.submitExam(id, payload);
      toast.success(res.data.message || 'Exam submitted');
      setConfirmOpen(false);
      load();
    } catch (e) {
      toast.error(e.message || 'Failed to submit exam');
      setConfirmOpen(false);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  if (submission) {
    const percentage = submission.totalMaxMarks > 0
      ? Math.round((submission.totalMarksObtained / submission.totalMaxMarks) * 100)
      : 0;
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
          <Button startIcon={<ArrowBackIcon />} variant="outlined" onClick={() => navigate('/exams')}>Back</Button>
          <Typography variant="h4" fontWeight={700}>My Result</Typography>
        </Box>
        <Paper sx={{ p: 3, mb: 3 }}>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3} alignItems="center">
            <Box sx={{ flex: 1 }}>
              <Typography variant="h5" fontWeight={600}>{exam?.name}</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                Submitted {new Date(submission.submittedAt).toLocaleString()}
              </Typography>
              <Stack direction="row" spacing={1} sx={{ mt: 1 }}>
                <Chip
                  label={submission.isGraded ? 'Graded' : 'Pending review'}
                  color={submission.isGraded ? 'success' : 'warning'} size="small"
                />
                {submission.isGraded && submission.gradedBy && (
                  <Chip label={`Graded by ${submission.gradedBy}`} size="small" variant="outlined" />
                )}
              </Stack>
            </Box>
            <Box sx={{ textAlign: 'center', minWidth: 160 }}>
              <Typography variant="h3" fontWeight={700}>
                {submission.totalMarksObtained} / {submission.totalMaxMarks}
              </Typography>
              <Typography variant="body2" color="text.secondary">{percentage}%</Typography>
            </Box>
          </Stack>
        </Paper>
        {submission.answers.map((a, idx) => (
          <Card key={a.id} sx={{ mb: 2 }}>
            <CardContent>
              <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                <Typography variant="body1" fontWeight={600}>{idx + 1}. {a.questionText || '(Image based question)'}</Typography>
                <Chip label={`${a.marks} marks`} size="small" variant="outlined" />
                <Chip label={`Obtained: ${a.marksObtained}`} size="small" color={a.marksObtained > 0 ? 'success' : 'default'} />
              </Stack>
              {a.questionType === 'MCQ' ? (
                <Typography variant="body2" color="text.secondary">
                  Your answer: {a.selectedOption || 'Not answered'}
                  {a.correctAnswer ? ` • Correct: ${a.correctAnswer}` : ''}
                </Typography>
              ) : (
                <Box>
                  <Typography variant="body2" sx={{ bgcolor: 'grey.100', p: 1.5, borderRadius: 1 }}>
                    {a.answerText || '(No text answer)'}
                  </Typography>
                  {a.imageUrl && (
                    <a href={a.imageUrl} target="_blank" rel="noreferrer">
                      <Button size="small" sx={{ mt: 1 }}>View answer image</Button>
                    </a>
                  )}
                  {a.remarks && (
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                      Teacher remarks: {a.remarks}
                    </Typography>
                  )}
                </Box>
              )}
            </CardContent>
          </Card>
        ))}
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} variant="outlined" onClick={() => navigate('/exams')}>Back</Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>{exam?.name}</Typography>
          <Stack direction="row" spacing={1} sx={{ mt: 1 }}>
            <Chip label={exam?.examType} color="primary" size="small" />
            <Chip label={exam?.className} color="secondary" size="small" variant="outlined" />
            <Chip label={`${questions.length} questions`} size="small" variant="outlined" />
            <Chip label={`Total ${exam?.totalMarks ?? 0} marks`} size="small" variant="outlined" />
          </Stack>
        </Box>
      </Box>

      {exam?.questionPaperUrl && (
        <Alert severity="info" sx={{ mb: 3 }} action={<Button size="small" href={exam.questionPaperUrl} target="_blank">Open</Button>}>
          Question paper available: {exam.questionPaperFileName || 'download here'}
        </Alert>
      )}

      {questions.map((q, idx) => (
        <Card key={q.id} sx={{ mb: 2 }}>
          <CardContent>
            <Stack direction="row" spacing={1} sx={{ mb: 1.5 }}>
              <Typography variant="body1" fontWeight={600}>{idx + 1}. {q.questionText || '(Image based question)'}</Typography>
              <Chip label={`${q.marks} marks`} size="small" variant="outlined" />
            </Stack>
            {q.imageUrl && (
              <a href={q.imageUrl} target="_blank" rel="noreferrer">
                <Button size="small" sx={{ mb: 1 }}>View question image</Button>
              </a>
            )}
            {q.questionType === 'MCQ' ? (
              <RadioGroup
                value={answers[q.id]?.selectedOption || ''}
                onChange={(e) => setAnswers({ ...answers, [q.id]: { ...(answers[q.id] || {}), selectedOption: e.target.value } })}
              >
                {[['A', q.optionA], ['B', q.optionB], ['C', q.optionC], ['D', q.optionD]].map(([opt, val]) =>
                  val ? <FormControlLabel key={opt} value={opt} control={<Radio size="small" />} label={`${opt}. ${val}`} /> : null
                )}
              </RadioGroup>
            ) : (
              <Box>
                <TextField
                  fullWidth label="Your answer" multiline rows={3} size="small"
                  value={answers[q.id]?.answerText || ''}
                  onChange={(e) => setAnswers({ ...answers, [q.id]: { ...(answers[q.id] || {}), answerText: e.target.value } })}
                />
                <Box sx={{ mt: 1.5 }}>
                  <Button
                    variant="outlined" component="label" size="small" startIcon={<UploadIcon />}
                    sx={{ mr: 1 }}
                  >
                    Upload Answer Image
                    <input type="file" hidden accept="image/*" onChange={(e) => handleImageAnswer(q.id, e)} />
                  </Button>
                  {answers[q.id]?.imageUrl && (
                    <Typography variant="body2" color="success.main" component="span">
                      Image attached
                    </Typography>
                  )}
                </Box>
              </Box>
            )}
          </CardContent>
        </Card>
      ))}

      <Stack direction="row" justifyContent="flex-end" sx={{ mt: 2 }}>
        <Button
          variant="contained" startIcon={<SendIcon />} size="large"
          disabled={questions.length === 0}
          onClick={() => setConfirmOpen(true)}
        >
          Submit Exam
        </Button>
      </Stack>

      <ConfirmDialog
        open={confirmOpen}
        title="Submit Exam"
        message={`Are you sure you want to submit "${exam?.name}"? You cannot change your answers after submission.`}
        confirmText="Submit"
        onConfirm={handleSubmit}
        onCancel={() => setConfirmOpen(false)}
        loading={submitting}
      />
      {submitting && <LinearProgress sx={{ mt: 2 }} />}
    </Box>
  );
}
