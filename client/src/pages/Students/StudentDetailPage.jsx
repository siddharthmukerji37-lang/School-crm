import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Paper, Typography, Avatar, Chip, Button, Divider, Tabs, Tab, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, CircularProgress, Alert, Link } from '@mui/material';
import Grid from '@mui/material/Grid2';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import PersonIcon from '@mui/icons-material/Person';
import EventAvailableIcon from '@mui/icons-material/EventAvailable';
import QuizIcon from '@mui/icons-material/Quiz';
import PaymentsIcon from '@mui/icons-material/Payments';
import FolderOpenIcon from '@mui/icons-material/FolderOpen';
import { fetchStudentById, clearSelectedStudent } from '../../store/slices/studentSlice';
import attendanceService from '../../services/attendanceService';
import examService from '../../services/examService';
import feeService from '../../services/feeService';
import studentService from '../../services/studentService';

function TabPanel({ children, value, index, ...other }) {
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

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

function formatDate(value) {
  if (!value) return '';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';
  const day = String(date.getDate()).padStart(2, '0');
  const month = date.toLocaleDateString(undefined, { month: 'short' });
  const year = date.getFullYear();
  return `${day} ${month} ${year}`;
}

function formatCurrency(value) {
  return `$${Number(value || 0).toFixed(2)}`;
}

function formatFileSize(bytes) {
  if (!bytes) return '-';
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

const attendanceStatusColor = (status) => {
  switch (status) {
    case 'Present': return 'success';
    case 'Absent': return 'error';
    case 'Late': return 'warning';
    case 'Excused': return 'info';
    default: return 'default';
  }
};

const feeStatusColor = (status) => {
  switch (status) {
    case 'Paid': return 'success';
    case 'PartiallyPaid': return 'warning';
    case 'Overdue': return 'error';
    default: return 'default';
  }
};

function LoadingBlock({ loading, children }) {
  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
        <CircularProgress />
      </Box>
    );
  }
  return children;
}

export default function StudentDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedStudent, loading } = useSelector((state) => state.students);
  const [tabValue, setTabValue] = useState(0);

  const [attendance, setAttendance] = useState({ items: [], loading: false, loaded: false });
  const [marks, setMarks] = useState({ items: [], loading: false, loaded: false });
  const [fees, setFees] = useState({ summary: null, loading: false, loaded: false });
  const [documents, setDocuments] = useState({ items: [], loading: false, loaded: false });

  useEffect(() => {
    dispatch(fetchStudentById(id));
    return () => {
      dispatch(clearSelectedStudent());
    };
  }, [dispatch, id]);

  useEffect(() => {
    if (tabValue === 1 && !attendance.loaded) {
      setAttendance((prev) => ({ ...prev, loading: true }));
      attendanceService
        .getStudentAttendance(id, { pageSize: 100 })
        .then((res) => setAttendance({ items: res.data.data?.items || [], loading: false, loaded: true }))
        .catch(() => setAttendance((prev) => ({ ...prev, loading: false, loaded: true })));
    }

    if (tabValue === 2 && !marks.loaded) {
      setMarks((prev) => ({ ...prev, loading: true }));
      examService
        .getStudentResults(id)
        .then((res) => setMarks({ items: res.data.data || [], loading: false, loaded: true }))
        .catch(() => setMarks((prev) => ({ ...prev, loading: false, loaded: true })));
    }

    if (tabValue === 3 && !fees.loaded) {
      setFees((prev) => ({ ...prev, loading: true }));
      feeService
        .getSummary(id)
        .then((res) => setFees({ summary: res.data.data || null, loading: false, loaded: true }))
        .catch(() => setFees((prev) => ({ ...prev, loading: false, loaded: true })));
    }

    if (tabValue === 4 && !documents.loaded) {
      setDocuments((prev) => ({ ...prev, loading: true }));
      studentService
        .getDocuments(id)
        .then((res) => setDocuments({ items: res.data.data || [], loading: false, loaded: true }))
        .catch(() => setDocuments((prev) => ({ ...prev, loading: false, loaded: true })));
    }
  }, [tabValue, id, attendance.loaded, marks.loaded, fees.loaded, documents.loaded]);

  if (loading || !selectedStudent) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const student = selectedStudent;

  const attendancePercentage = attendance.items.length
    ? Math.round((attendance.items.filter((a) => a.status === 'Present' || a.status === 'Late').length / attendance.items.length) * 100)
    : 0;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/students')}
          variant="outlined"
        >
          Back
        </Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>
            Student Profile
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<EditIcon />}
          onClick={() => navigate(`/students/${id}/edit`)}
        >
          Edit
        </Button>
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, flexWrap: 'wrap' }}>
          <Avatar
            sx={{
              width: 80,
              height: 80,
              bgcolor: 'primary.main',
              fontSize: '1.75rem',
              fontWeight: 700,
            }}
          >
            {student.firstName?.charAt(0)}
            {student.lastName?.charAt(0)}
          </Avatar>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h5" fontWeight={600}>
              {student.firstName} {student.lastName}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
              <Chip label={student.className || 'N/A'} color="primary" size="small" />
              <Chip label={student.sectionName || 'N/A'} color="secondary" size="small" />
              <Chip
                label={student.status || 'Active'}
                color={student.status === 'Inactive' ? 'default' : 'success'}
                size="small"
                variant="outlined"
              />
            </Box>
          </Box>
        </Box>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Tabs
          value={tabValue}
          onChange={(_, newValue) => setTabValue(newValue)}
          variant="scrollable"
          scrollButtons="auto"
          sx={{
            borderBottom: 1,
            borderColor: 'divider',
            mb: 0,
            '& .MuiTab-root': { minHeight: 48 },
          }}
        >
          <Tab icon={<PersonIcon />} iconPosition="start" label="Profile" />
          <Tab icon={<EventAvailableIcon />} iconPosition="start" label="Attendance" />
          <Tab icon={<QuizIcon />} iconPosition="start" label="Marks" />
          <Tab icon={<PaymentsIcon />} iconPosition="start" label="Fees" />
          <Tab icon={<FolderOpenIcon />} iconPosition="start" label="Documents" />
        </Tabs>

        <TabPanel value={tabValue} index={0}>
          <Grid container spacing={3}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
                Personal Information
              </Typography>
              <Divider sx={{ mb: 1 }} />
              <DetailRow label="First Name" value={student.firstName} />
              <DetailRow label="Last Name" value={student.lastName} />
              <DetailRow label="Email" value={student.email} />
              <DetailRow label="Phone" value={student.phone} />
              <DetailRow label="Date of Birth" value={formatDate(student.dateOfBirth)} />
              <DetailRow label="Gender" value={student.gender} />
              <DetailRow label="Blood Group" value={student.bloodGroup} />
              <DetailRow label="Address" value={student.address} />
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
                Academic Information
              </Typography>
              <Divider sx={{ mb: 1 }} />
              <DetailRow label="Admission Number" value={student.admissionNumber} />
              <DetailRow label="Admission Date" value={formatDate(student.admissionDate)} />
              <DetailRow label="Class" value={student.className} />
              <DetailRow label="Section" value={student.sectionName} />

              <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ mt: 2, color: 'primary.main' }}>
                Parent/Guardian Information
              </Typography>
              <Divider sx={{ mb: 1 }} />
              <DetailRow label="Parent Name" value={student.parentName} />
              <DetailRow label="Parent Phone" value={student.parentPhone} />
              <DetailRow label="Parent Email" value={student.parentEmail} />
            </Grid>
            <Grid size={{ xs: 12 }}>
              <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
                Additional Information
              </Typography>
              <Divider sx={{ mb: 1 }} />
              <DetailRow label="Transport Required" value={student.transportRequired ? 'Yes' : 'No'} />
              <DetailRow label="Hostel Required" value={student.hostelRequired ? 'Yes' : 'No'} />
              <DetailRow label="Notes" value={student.notes} />
            </Grid>
          </Grid>
        </TabPanel>

        <TabPanel value={tabValue} index={1}>
          <LoadingBlock loading={attendance.loading}>
            {attendance.items.length === 0 ? (
              <Alert severity="info">No attendance records found.</Alert>
            ) : (
              <Box>
                <Paper variant="outlined" sx={{ p: 2, mb: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
                  <Typography variant="body2" color="text.secondary" fontWeight={500}>
                    Attendance (last 6 months):
                  </Typography>
                  <Typography variant="h6" fontWeight={700} color={attendancePercentage >= 75 ? 'success.main' : 'error.main'}>
                    {attendancePercentage}%
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {attendance.items.length} records
                  </Typography>
                </Paper>
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Date</TableCell>
                        <TableCell>Status</TableCell>
                        <TableCell>Remarks</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {attendance.items.map((a) => (
                        <TableRow key={a.id} hover>
                          <TableCell>{formatDate(a.date)}</TableCell>
                          <TableCell>
                            <Chip label={a.status} color={attendanceStatusColor(a.status)} size="small" />
                          </TableCell>
                          <TableCell>{a.remarks || '-'}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Box>
            )}
          </LoadingBlock>
        </TabPanel>

        <TabPanel value={tabValue} index={2}>
          <LoadingBlock loading={marks.loading}>
            {marks.items.length === 0 ? (
              <Alert severity="info">No exam marks found for this student.</Alert>
            ) : (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                {marks.items.map((result) => (
                  <Paper key={result.examId} variant="outlined" sx={{ p: 2 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', mb: 1 }}>
                      <Typography variant="subtitle1" fontWeight={600}>
                        {result.examName}
                      </Typography>
                      <Chip
                        label={result.isPassed ? 'Passed' : 'Failed'}
                        color={result.isPassed ? 'success' : 'error'}
                        size="small"
                      />
                    </Box>
                    <TableContainer>
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Subject</TableCell>
                            <TableCell align="center">Marks</TableCell>
                            <TableCell align="center">Max</TableCell>
                            <TableCell align="center">Pass</TableCell>
                            <TableCell align="center">Result</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {(result.subjectResults || []).map((s) => (
                            <TableRow key={s.subjectId} hover>
                              <TableCell>{s.subjectName}</TableCell>
                              <TableCell align="center">{s.marksObtained}</TableCell>
                              <TableCell align="center">{s.maxMarks}</TableCell>
                              <TableCell align="center">{s.passingMarks}</TableCell>
                              <TableCell align="center">
                                <Chip
                                  label={s.isPass ? 'Pass' : 'Fail'}
                                  color={s.isPass ? 'success' : 'error'}
                                  size="small"
                                  variant="outlined"
                                />
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                    <Box sx={{ display: 'flex', gap: 3, mt: 2, flexWrap: 'wrap' }}>
                      <Typography variant="body2">
                        Total: <b>{result.totalMarksObtained}</b> / {result.totalMaxMarks}
                      </Typography>
                      <Typography variant="body2">
                        Percentage: <b>{result.percentage}%</b>
                      </Typography>
                      {result.grade && (
                        <Typography variant="body2">
                          Grade: <b>{result.grade}</b>
                        </Typography>
                      )}
                    </Box>
                  </Paper>
                ))}
              </Box>
            )}
          </LoadingBlock>
        </TabPanel>

        <TabPanel value={tabValue} index={3}>
          <LoadingBlock loading={fees.loading}>
            {!fees.summary || !fees.summary.installments?.length ? (
              <Alert severity="info">No fee records found for this student.</Alert>
            ) : (
              <Box>
                <Grid container spacing={2} sx={{ mb: 3 }}>
                  <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Paper variant="outlined" sx={{ p: 2 }}>
                      <Typography variant="body2" color="text.secondary">Total Fee</Typography>
                      <Typography variant="h6" fontWeight={700}>{formatCurrency(fees.summary.totalFeeAmount)}</Typography>
                    </Paper>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Paper variant="outlined" sx={{ p: 2 }}>
                      <Typography variant="body2" color="text.secondary">Paid</Typography>
                      <Typography variant="h6" fontWeight={700} color="success.main">{formatCurrency(fees.summary.totalPaidAmount)}</Typography>
                    </Paper>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Paper variant="outlined" sx={{ p: 2 }}>
                      <Typography variant="body2" color="text.secondary">Pending</Typography>
                      <Typography variant="h6" fontWeight={700} color="error.main">{formatCurrency(fees.summary.totalPendingAmount)}</Typography>
                    </Paper>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Paper variant="outlined" sx={{ p: 2 }}>
                      <Typography variant="body2" color="text.secondary">Fine</Typography>
                      <Typography variant="h6" fontWeight={700} color="warning.main">{formatCurrency(fees.summary.totalFineAmount)}</Typography>
                    </Paper>
                  </Grid>
                </Grid>
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Installment</TableCell>
                        <TableCell>Due Date</TableCell>
                        <TableCell align="right">Amount</TableCell>
                        <TableCell align="right">Paid</TableCell>
                        <TableCell align="right">Pending</TableCell>
                        <TableCell>Status</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {fees.summary.installments.map((inst) => (
                        <TableRow key={inst.installmentId} hover>
                          <TableCell>{inst.name}</TableCell>
                          <TableCell>{formatDate(inst.dueDate)}</TableCell>
                          <TableCell align="right">{formatCurrency(inst.amount)}</TableCell>
                          <TableCell align="right">{formatCurrency(inst.paidAmount)}</TableCell>
                          <TableCell align="right">{formatCurrency(inst.pendingAmount)}</TableCell>
                          <TableCell>
                            <Chip label={inst.status} color={feeStatusColor(inst.status)} size="small" />
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              </Box>
            )}
          </LoadingBlock>
        </TabPanel>

        <TabPanel value={tabValue} index={4}>
          <LoadingBlock loading={documents.loading}>
            {documents.items.length === 0 ? (
              <Alert severity="info">No documents found for this student.</Alert>
            ) : (
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Document Name</TableCell>
                      <TableCell>Type</TableCell>
                      <TableCell>File</TableCell>
                      <TableCell align="right">Size</TableCell>
                      <TableCell>Uploaded</TableCell>
                      <TableCell>Download</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {documents.items.map((doc) => (
                      <TableRow key={doc.id} hover>
                        <TableCell>{doc.documentName}</TableCell>
                        <TableCell>{doc.documentType}</TableCell>
                        <TableCell>{doc.fileName || '-'}</TableCell>
                        <TableCell align="right">{formatFileSize(doc.fileSize)}</TableCell>
                        <TableCell>{formatDate(doc.uploadedAt)}</TableCell>
                        <TableCell>
                          <Link href={doc.fileUrl} target="_blank" rel="noopener noreferrer" underline="hover">
                            Open
                          </Link>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </LoadingBlock>
        </TabPanel>
      </Paper>
    </Box>
  );
}
