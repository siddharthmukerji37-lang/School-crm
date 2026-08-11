import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  Box, Paper, Typography, Chip, Button, Stack, TextField, Divider,
  CircularProgress, Card, CardContent, Grid,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import SaveIcon from '@mui/icons-material/Save';
import toast from 'react-hot-toast';
import examService from '../../services/examService';
import DataTable from '../../components/common/DataTable';

export default function ExamSubmissionsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [submissions, setSubmissions] = useState([]);
  const [loading, setLoading] = useState(true);
  const [exam, setExam] = useState(null);
  const [selected, setSelected] = useState(null);
  const [grades, setGrades] = useState({});
  const [saving, setSaving] = useState(false);

  const load = async () => {
    try {
      const [subRes, examRes] = await Promise.all([
        examService.getSubmissions(id),
        examService.getById(id),
      ]);
      setSubmissions(subRes.data.data || []);
      setExam(examRes.data.data);
    } catch (e) {
      toast.error(e.message || 'Failed to load submissions');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, [id]);

  const openGrade = (sub) => {
    setSelected(sub);
    const initial = {};
    sub.answers.forEach((a) => {
      initial[a.id] = { marks: a.marksObtained ?? '', remarks: a.remarks || '' };
    });
    setGrades(initial);
  };

  const handleGrade = async () => {
    const payload = selected.answers
      .filter((a) => a.questionType === 'Descriptive')
      .map((a) => ({
        answerId: a.id,
        marksObtained: Number(grades[a.id]?.marks) || 0,
        remarks: grades[a.id]?.remarks || null,
      }));
    if (payload.length === 0) {
      toast.success('Submission already fully auto-graded.');
      setSelected(null);
      return;
    }
    setSaving(true);
    try {
      await examService.gradeSubmission(selected.id, { answers: payload });
      toast.success('Submission graded successfully');
      setSelected(null);
      load();
    } catch (e) {
      toast.error(e.message || 'Failed to grade submission');
    } finally {
      setSaving(false);
    }
  };

  const columns = [
    { id: 'student', header: 'Student', accessor: 'studentName', minWidth: 180 },
    { id: 'admission', header: 'Adm No', accessor: 'admissionNumber', minWidth: 110 },
    { id: 'submittedAt', header: 'Submitted At', accessor: 'submittedAt', minWidth: 170 },
    {
      id: 'marks', header: 'Marks', accessor: 'totalMarksObtained', minWidth: 120,
      render: (v, row) => <Typography variant="body2">{v} / {row.totalMaxMarks}</Typography>,
    },
    {
      id: 'status', header: 'Status', accessor: 'isGraded', minWidth: 110,
      render: (v) => (
        <Chip label={v ? 'Graded' : 'Pending Review'} color={v ? 'success' : 'warning'} size="small" variant="outlined" />
      ),
    },
  ];

  if (loading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  if (selected) {
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
          <Button startIcon={<ArrowBackIcon />} variant="outlined" onClick={() => setSelected(null)}>Back</Button>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h4" fontWeight={700}>Grade Submission</Typography>
            <Typography variant="body2" color="text.secondary">
              {selected.studentName} • {selected.admissionNumber} • submitted {new Date(selected.submittedAt).toLocaleString()}
            </Typography>
          </Box>
          <Button variant="contained" startIcon={<SaveIcon />} disabled={saving} onClick={handleGrade}>
            {saving ? 'Saving...' : 'Save Grades'}
          </Button>
        </Box>

        {selected.answers.map((a, idx) => (
          <Card key={a.id} sx={{ mb: 2 }}>
            <CardContent>
              <Stack direction="row" spacing={1} sx={{ mb: 1 }}>
                <Typography variant="body1" fontWeight={600}>{idx + 1}. {a.questionText || '(Image based question)'}</Typography>
                <Chip label={a.questionType} size="small" color={a.questionType === 'MCQ' ? 'info' : 'warning'} variant="outlined" />
                <Chip label={`Max ${a.marks}`} size="small" variant="outlined" />
                {a.questionType === 'MCQ' && (
                  <Chip
                    label={a.isCorrect ? 'Correct' : 'Incorrect'}
                    size="small" color={a.isCorrect ? 'success' : 'error'}
                  />
                )}
              </Stack>
              {a.imageUrl && (
                <a href={a.imageUrl} target="_blank" rel="noreferrer">
                  <Button size="small">View question image</Button>
                </a>
              )}
              {a.questionType === 'MCQ' ? (
                <Box sx={{ pl: 1, mt: 1 }}>
                  {[['A', a.optionA], ['B', a.optionB], ['C', a.optionC], ['D', a.optionD]].map(([opt, val]) =>
                    val ? (
                      <Typography key={opt} variant="body2"
                        color={a.selectedOption === opt ? 'primary.main' : 'text.secondary'}>
                        {opt}. {val} {a.selectedOption === opt ? ' (selected)' : ''}{a.correctAnswer === opt ? ' ✓' : ''}
                      </Typography>
                    ) : null
                  )}
                </Box>
              ) : (
                <Box sx={{ mt: 1 }}>
                  <Typography variant="body2" sx={{ bgcolor: 'grey.100', p: 1.5, borderRadius: 1 }}>
                    {a.answerText || '(No text answer)'}
                  </Typography>
                  {a.imageUrl && (
                    <Box sx={{ mt: 1 }}>
                      <a href={a.imageUrl} target="_blank" rel="noreferrer">
                        <Button size="small">View student image answer</Button>
                      </a>
                    </Box>
                  )}
                  <Grid container spacing={2} sx={{ mt: 1 }}>
                    <Grid size={{ xs: 12, sm: 4 }}>
                      <TextField
                        fullWidth label="Marks Obtained" type="number" size="small"
                        value={grades[a.id]?.marks ?? ''}
                        onChange={(e) => setGrades({ ...grades, [a.id]: { ...grades[a.id], marks: e.target.value } })}
                      />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 8 }}>
                      <TextField
                        fullWidth label="Remarks" size="small"
                        value={grades[a.id]?.remarks ?? ''}
                        onChange={(e) => setGrades({ ...grades, [a.id]: { ...grades[a.id], remarks: e.target.value } })}
                      />
                    </Grid>
                  </Grid>
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
        <Button startIcon={<ArrowBackIcon />} variant="outlined" onClick={() => navigate(`/exams/${id}/questions`)}>Back</Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>Submissions — {exam?.name || ''}</Typography>
        </Box>
      </Box>
      <Paper>
        <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider' }}>
          <Typography variant="body2" color="text.secondary">
            Click a submission to review and grade descriptive answers.
          </Typography>
        </Box>
        <DataTable
          columns={columns}
          rows={submissions}
          loading={false}
          searchPlaceholder="Search students..."
          onView={(row) => openGrade(row)}
          showActions
          emptyMessage="No submissions yet"
          enableSearch={false}
        />
      </Paper>
    </Box>
  );
}
