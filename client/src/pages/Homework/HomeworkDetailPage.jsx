import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import {
  Box, Paper, Typography, Grid, Chip, Button, Divider, CircularProgress,
  TextField, Stack, Card, CardContent, Alert, LinearProgress, Dialog, DialogTitle,
  DialogContent, DialogActions,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import UploadIcon from '@mui/icons-material/Upload';
import SendIcon from '@mui/icons-material/Send';
import LinkIcon from '@mui/icons-material/Link';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import CancelIcon from '@mui/icons-material/Cancel';
import { fetchHomeworkById, clearSelectedHomework } from '../../store/slices/homeworkSlice';
import homeworkService from '../../services/homeworkService';
import { uploadFile } from '../../utils/upload';
import toast from 'react-hot-toast';

const approvalColor = (s) => {
  switch (s) { case 'Approved': return 'success'; case 'Rejected': return 'error'; default: return 'warning'; }
};

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

export default function HomeworkDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedHomework, loading } = useSelector((state) => state.homework);
  const { user } = useSelector((state) => state.auth);
  const roles = user?.roles || [];
  const isAdmin = roles.some((r) => r === 'SuperAdmin' || r === 'Admin');
  const isTeacher = roles.some((r) => r === 'Teacher' || r === 'ClassTeacher');
  const isStudent = roles.some((r) => r === 'Student');

  const [mySubmission, setMySubmission] = useState(null);
  const [subText, setSubText] = useState('');
  const [subAttachment, setSubAttachment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [submittingForApproval, setSubmittingForApproval] = useState(false);

  const [reviewTarget, setReviewTarget] = useState(null);
  const [reviewMode, setReviewMode] = useState('correct');
  const [reviewMarks, setReviewMarks] = useState('');
  const [reviewRemarks, setReviewRemarks] = useState('');
  const [reviewing, setReviewing] = useState(false);

  const openReviewDialog = (submission, mode) => {
    setReviewTarget(submission);
    setReviewMode(mode);
    setReviewMarks(submission.marks != null ? String(submission.marks) : '');
    setReviewRemarks('');
  };

  const submitReview = async () => {
    if (!reviewTarget) return;
    setReviewing(true);
    try {
      if (reviewMode === 'correct') {
        await homeworkService.review(reviewTarget.id, {
          marks: Number(reviewMarks),
          remarks: reviewRemarks,
        });
        toast.success('Marked as correct');
      } else {
        await homeworkService.reject(reviewTarget.id, { remarks: reviewRemarks });
        toast.success('Returned to student for resubmission');
      }
      setReviewTarget(null);
      dispatch(fetchHomeworkById(id));
    } catch (e) {
      toast.error(e.message || 'Review failed');
    } finally {
      setReviewing(false);
    }
  };

  useEffect(() => {
    dispatch(fetchHomeworkById(id));
    return () => {
      dispatch(clearSelectedHomework());
    };
  }, [dispatch, id]);

  useEffect(() => {
    if (!isStudent) return;
    (async () => {
      try {
        const res = await homeworkService.getAssignments({ pageSize: 100 });
        const mine = (res.data?.data?.items || []).find((a) => a.homeworkId === id);
        setMySubmission(mine || null);
      } catch { /* ignore */ }
    })();
  }, [id, isStudent]);

  if (loading || !selectedHomework) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const hw = selectedHomework;

  const handleAttachment = async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      const res = await uploadFile(file);
      if (res.success) {
        setSubAttachment(res.url);
        toast.success('Attachment uploaded');
      } else {
        toast.error(res.message || 'Upload failed');
      }
    } catch (err) {
      toast.error(err.message || 'Upload failed');
    }
  };

  const submitAssignment = async () => {
    setSubmitting(true);
    try {
      const payload = { homeworkId: id, submissionText: subText || null, attachmentUrl: subAttachment || null };
      const res = await homeworkService.submit(id, payload);
      toast.success(res.data.message || 'Submitted');
      const list = await homeworkService.getAssignments({ pageSize: 100 });
      const mine = (list.data?.data?.items || []).find((a) => a.homeworkId === id);
      setMySubmission(mine || { submitted: true });
    } catch (e) {
      toast.error(e.message || 'Failed to submit');
    } finally {
      setSubmitting(false);
    }
  };

  const requestApproval = async () => {
    setSubmittingForApproval(true);
    try {
      await homeworkService.submitForApproval(id);
      toast.success('Submitted for approval');
      dispatch(fetchHomeworkById(id));
    } catch (e) {
      toast.error(e.message || 'Failed');
    } finally {
      setSubmittingForApproval(false);
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/homework')} variant="outlined">
          Back
        </Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>
            Homework Details
          </Typography>
        </Box>
        {isAdmin && (
          <Button
            variant="contained"
            startIcon={<EditIcon />}
            onClick={() => navigate(`/homework/${id}/edit`)}
          >
            Edit
          </Button>
        )}
        {isTeacher && hw.approvalStatus !== 'Approved' && (
          <Button variant="contained" onClick={requestApproval} disabled={submittingForApproval}>
            {submittingForApproval ? 'Submitting...' : 'Submit for Approval'}
          </Button>
        )}
      </Box>

      {hw.rejectionReason && (
        <Alert severity="error" sx={{ mb: 3 }}>
          Rejected: {hw.rejectionReason}
        </Alert>
      )}

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h5" fontWeight={600}>
              {hw.title}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
              <Chip label={hw.subjectName || 'No Subject'} color="primary" size="small" />
              <Chip label={hw.className || 'No Class'} color="secondary" size="small" variant="outlined" />
              <Chip label={`Approval: ${hw.approvalStatus || 'Pending'}`} color={approvalColor(hw.approvalStatus)} size="small" variant="outlined" />
              <Chip
                label={hw.isActive ? 'Active' : 'Inactive'}
                color={hw.isActive ? 'success' : 'default'}
                size="small"
                variant="outlined"
              />
            </Box>
          </Box>
        </Box>
      </Paper>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
          Assignment Information
        </Typography>
        <Divider sx={{ mb: 1 }} />
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Description" value={hw.description} />
            <DetailRow label="Subject" value={hw.subjectName} />
            <DetailRow label="Class" value={hw.className} />
            <DetailRow label="Section" value={hw.sectionName} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Assigned By" value={hw.teacherName} />
            <DetailRow label="Assigned Date" value={hw.assignedDate} />
            <DetailRow label="Due Date" value={hw.dueDate} />
            <DetailRow label="Submissions" value={hw.submissionCount ?? 0} />
          </Grid>
        </Grid>
        {hw.attachmentUrl && (
          <Box sx={{ mt: 2 }}>
            <Button size="small" variant="outlined" startIcon={<LinkIcon />} href={hw.attachmentUrl} target="_blank" rel="noreferrer">
              View Homework Attachment
            </Button>
          </Box>
        )}
      </Paper>

      {(isTeacher || isAdmin) && hw.submissions?.length > 0 && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
            Student Submissions ({hw.submissions.length})
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Stack spacing={2}>
            {hw.submissions.map((s) => (
              <Card key={s.id} variant="outlined" sx={{ bgcolor: 'grey.50' }}>
                <CardContent>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <Box>
                      <Typography variant="body2" fontWeight={600}>{s.studentName || 'Student'}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        Submitted {s.submittedDate ? new Date(s.submittedDate).toLocaleString() : 'yes'}
                      </Typography>
                    </Box>
                    <Chip
                      label={s.status}
                      color={s.marks != null ? 'success' : 'info'}
                      size="small"
                      variant="outlined"
                    />
                  </Box>
                  {s.submissionText && (
                    <Typography variant="body2" sx={{ mt: 1, whiteSpace: 'pre-wrap' }}>{s.submissionText}</Typography>
                  )}
                  {s.attachmentUrl && (
                    <Button size="small" sx={{ mt: 1 }} href={s.attachmentUrl} target="_blank" rel="noreferrer" startIcon={<LinkIcon />}>
                      View uploaded answer
                    </Button>
                  )}
                  {s.marks != null && (
                    <Typography variant="body2" sx={{ mt: 1 }} fontWeight={600}>
                      Marks: {s.marks} {s.remarks ? ` • ${s.remarks}` : ''}
                    </Typography>
                  )}
                  {(isTeacher || isAdmin) && s.status === 'Submitted' && (
                    <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
                      <Button
                        size="small"
                        variant="contained"
                        color="success"
                        startIcon={<CheckCircleIcon />}
                        onClick={() => openReviewDialog(s, 'correct')}
                      >
                        Correct
                      </Button>
                      <Button
                        size="small"
                        variant="outlined"
                        color="error"
                        startIcon={<CancelIcon />}
                        onClick={() => openReviewDialog(s, 'not-correct')}
                      >
                        Not Correct
                      </Button>
                    </Stack>
                  )}
                </CardContent>
              </Card>
            ))}
          </Stack>
        </Paper>
      )}

      {isStudent && (
        <Paper sx={{ p: 3 }}>
          <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
            My Submission
          </Typography>
          <Divider sx={{ mb: 2 }} />
          {mySubmission?.status === 'Rejected' && (
            <Alert severity="warning" sx={{ mb: 2 }}>
              Your homework was returned: {mySubmission.remarks || 'Please fix it and resubmit.'}
            </Alert>
          )}
          {mySubmission && mySubmission.status !== 'Rejected' ? (
            <Card variant="outlined" sx={{ bgcolor: 'grey.50' }}>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Submitted {mySubmission.submittedDate ? new Date(mySubmission.submittedDate).toLocaleString() : 'yes'}
                </Typography>
                {mySubmission.submissionText && (
                  <Typography variant="body2" sx={{ mt: 1 }}>{mySubmission.submissionText}</Typography>
                )}
                {mySubmission.attachmentUrl && (
                  <Button size="small" sx={{ mt: 1 }} href={mySubmission.attachmentUrl} target="_blank" rel="noreferrer">
                    View attachment
                  </Button>
                )}
                {mySubmission.status && (
                  <Chip label={`Status: ${mySubmission.status}`} color={mySubmission.marks != null ? 'success' : 'info'} size="small" sx={{ mt: 1 }} variant="outlined" />
                )}
                {mySubmission.marks != null && (
                  <Typography variant="body2" sx={{ mt: 1 }} fontWeight={600}>
                    Marks: {mySubmission.marks} {mySubmission.remarks ? ` • ${mySubmission.remarks}` : ''}
                  </Typography>
                )}
              </CardContent>
            </Card>
          ) : (
            <Stack spacing={2}>
              <TextField
                label="Your answer" multiline rows={4} size="small"
                value={subText}
                onChange={(e) => setSubText(e.target.value)}
              />
              <Box>
                <Button variant="outlined" component="label" startIcon={<UploadIcon />} sx={{ mr: 1 }}>
                  Upload Attachment
                  <input type="file" hidden onChange={handleAttachment} />
                </Button>
                {subAttachment && <Typography variant="body2" color="success.main" component="span">Attached</Typography>}
              </Box>
              <Stack direction="row" justifyContent="flex-end">
                <Button variant="contained" startIcon={<SendIcon />} onClick={submitAssignment} disabled={submitting}>
                  {submitting ? 'Submitting...' : 'Submit Homework'}
                </Button>
              </Stack>
            </Stack>
          )}
          {submitting && <LinearProgress sx={{ mt: 2 }} />}
        </Paper>
      )}

      <Dialog open={Boolean(reviewTarget)} onClose={() => !reviewing && setReviewTarget(null)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {reviewMode === 'correct' ? 'Mark as Correct' : 'Mark as Not Correct'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            {reviewMode === 'correct' && (
              <TextField
                label="Marks" type="number" fullWidth size="small"
                value={reviewMarks}
                onChange={(e) => setReviewMarks(e.target.value)}
              />
            )}
            <TextField
              label={reviewMode === 'correct' ? 'Remarks (optional)' : 'Reason for return (required)'}
              fullWidth multiline rows={3} size="small"
              value={reviewRemarks}
              onChange={(e) => setReviewRemarks(e.target.value)}
            />
            {reviewMode === 'not-correct' && (
              <Typography variant="caption" color="text.secondary">
                The student will be notified to resubmit the homework.
              </Typography>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setReviewTarget(null)} disabled={reviewing}>Cancel</Button>
          <Button
            variant="contained"
            color={reviewMode === 'correct' ? 'success' : 'error'}
            onClick={submitReview}
            disabled={reviewing || (reviewMode === 'not-correct' && !reviewRemarks.trim())}
          >
            {reviewing ? 'Saving...' : reviewMode === 'correct' ? 'Save & Notify' : 'Return for Resubmit'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
