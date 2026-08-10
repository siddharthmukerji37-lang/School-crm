import React, { useState, useEffect } from 'react';
import {
  Box,
  Button,
  Typography,
  Grid,
  Card,
  CardContent,
  CardActions,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  TextField,
  Stack,
  Avatar,
} from '@mui/material';
import SchoolIcon from '@mui/icons-material/School';
import EventAvailableIcon from '@mui/icons-material/EventAvailable';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import QuizIcon from '@mui/icons-material/Quiz';
import PeopleIcon from '@mui/icons-material/People';
import InventoryIcon from '@mui/icons-material/Inventory';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import DownloadIcon from '@mui/icons-material/Download';
import PageHeader from '../../components/common/PageHeader';
import axiosInstance from '../../services/axiosInstance';
import toast from 'react-hot-toast';

const reportTypes = [
  {
    id: 'students',
    title: 'Student Report',
    description: 'Generate reports on student enrollment, demographics, and academic performance.',
    icon: <SchoolIcon />,
    color: '#1976d2',
    filters: ['class', 'section'],
  },
  {
    id: 'attendance',
    title: 'Attendance Report',
    description: 'View attendance records, trends, and summary statistics.',
    icon: <EventAvailableIcon />,
    color: '#388e3c',
    filters: ['dateRange', 'class', 'section'],
  },
  {
    id: 'fee',
    title: 'Fee Report',
    description: 'Analyze fee collection, outstanding payments, and financial summaries.',
    icon: <AttachMoneyIcon />,
    color: '#f57c00',
    filters: ['dateRange', 'class'],
  },
  {
    id: 'exam',
    title: 'Exam Report',
    description: 'View examination results, grades, and performance analysis.',
    icon: <QuizIcon />,
    color: '#7b1fa2',
    filters: ['class', 'exam'],
  },
  {
    id: 'employee',
    title: 'Employee Report',
    description: 'Generate reports on staff, departments, and employment details.',
    icon: <PeopleIcon />,
    color: '#00838f',
    filters: [],
  },
  {
    id: 'inventory',
    title: 'Inventory Report',
    description: 'Track inventory levels, stock movements, and asset valuation.',
    icon: <InventoryIcon />,
    color: '#5d4037',
    filters: [],
  },
  {
    id: 'account',
    title: 'Account Report',
    description: 'Financial reports including income, expenditure, and balance sheets.',
    icon: <AccountBalanceIcon />,
    color: '#c62828',
    filters: ['dateRange'],
  },
];

export default function ReportsPage() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedReport, setSelectedReport] = useState(null);
  const [format, setFormat] = useState('pdf');
  const [filters, setFilters] = useState({
    fromDate: '',
    toDate: '',
    classRoomId: '',
    sectionId: '',
    examId: '',
  });
  const [generating, setGenerating] = useState(false);

  const [schoolId, setSchoolId] = useState('');
  const [classes, setClasses] = useState([]);
  const [sections, setSections] = useState([]);
  const [exams, setExams] = useState([]);

  useEffect(() => {
    const fetchMeta = async () => {
      try {
        const res = await axiosInstance.get('/schools');
        const items = res.data.data?.items || res.data.data || [];
        if (items.length === 0) return;
        setSchoolId(items[0].id);
        const classRes = await axiosInstance.get(`/schools/${items[0].id}/classes`);
        setClasses(classRes.data.data || []);
      } catch {}
    };
    fetchMeta();
  }, []);

  useEffect(() => {
    setSections([]);
    setFilters((prev) => ({ ...prev, sectionId: '' }));
    if (!filters.classRoomId) return;
    const fetchSections = async () => {
      try {
        const res = await axiosInstance.get(`/schools/classes/${filters.classRoomId}/sections`);
        setSections(res.data.data || []);
      } catch {
        setSections([]);
      }
    };
    fetchSections();
  }, [filters.classRoomId]);

  const loadExams = async () => {
    try {
      const res = await axiosInstance.get('/exams', { params: { pageSize: 100 } });
      setExams(res.data.data?.items || []);
    } catch {
      setExams([]);
    }
  };

  const handleOpenDialog = (report) => {
    setSelectedReport(report);
    setFormat('pdf');
    setFilters({
      fromDate: '',
      toDate: '',
      classRoomId: '',
      sectionId: '',
      examId: '',
    });
    if (report.filters.includes('exam')) loadExams();
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setSelectedReport(null);
  };

  const handleFilterChange = (field, value) => {
    setFilters((prev) => ({ ...prev, [field]: value }));
  };

  const handleGenerate = async () => {
    if (!selectedReport) return;

    const reportFilters = selectedReport.filters || [];
    if (reportFilters.includes('dateRange') && (!filters.fromDate || !filters.toDate)) {
      toast.error('Please select start and end date');
      return;
    }
    if (reportFilters.includes('exam') && !filters.examId) {
      toast.error('Please select an exam');
      return;
    }

    setGenerating(true);
    try {
      const params = { format };
      if (filters.fromDate) params.fromDate = filters.fromDate;
      if (filters.toDate) params.toDate = filters.toDate;
      if (filters.classRoomId) params.classRoomId = filters.classRoomId;
      if (filters.sectionId) params.sectionId = filters.sectionId;
      if (filters.examId) params.examId = filters.examId;
      if (schoolId) params.schoolId = schoolId;

      const token = localStorage.getItem('token');
      const baseUrl = import.meta.env.VITE_API_BASE_URL || '/api';
      const queryString = new URLSearchParams(params).toString();
      const response = await fetch(`${baseUrl}/reports/${selectedReport.id}?${queryString}`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Failed to generate report');
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${selectedReport.title.replace(/\s+/g, '_')}.${format}`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      toast.success(`${selectedReport.title} generated successfully`);
      handleCloseDialog();
    } catch (error) {
      toast.error(error.message || 'Failed to generate report');
    } finally {
      setGenerating(false);
    }
  };

  const reportFilters = selectedReport?.filters || [];
  const showDateRange = reportFilters.includes('dateRange');
  const showClass = reportFilters.includes('class');
  const showSection = reportFilters.includes('section');
  const showExam = reportFilters.includes('exam');

  return (
    <Box>
      <PageHeader title="Reports" subtitle="Generate and download various school reports" />

      <Grid container spacing={3}>
        {reportTypes.map((report) => (
          <Grid size={{ xs: 12, sm: 6, md: 4 }} key={report.id}>
            <Card
              sx={{
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                transition: 'transform 0.2s, box-shadow 0.2s',
                '&:hover': {
                  transform: 'translateY(-4px)',
                  boxShadow: 4,
                },
              }}
            >
              <CardContent sx={{ flex: 1 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <Avatar
                    sx={{
                      bgcolor: report.color,
                      color: '#fff',
                      width: 48,
                      height: 48,
                      mr: 2,
                    }}
                  >
                    {report.icon}
                  </Avatar>
                  <Typography variant="h6" fontWeight={600}>
                    {report.title}
                  </Typography>
                </Box>
                <Typography variant="body2" color="text.secondary">
                  {report.description}
                </Typography>
              </CardContent>
              <CardActions sx={{ px: 2, pb: 2 }}>
                <Button
                  variant="contained"
                  startIcon={<DownloadIcon />}
                  onClick={() => handleOpenDialog(report)}
                  fullWidth
                >
                  Generate
                </Button>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 600 }}>
          Generate {selectedReport?.title}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <FormControl fullWidth>
              <InputLabel>Format</InputLabel>
              <Select
                value={format}
                label="Format"
                onChange={(e) => setFormat(e.target.value)}
              >
                <MenuItem value="pdf">PDF</MenuItem>
                <MenuItem value="excel">Excel</MenuItem>
                <MenuItem value="csv">CSV</MenuItem>
              </Select>
            </FormControl>

            {showDateRange && (
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField
                  label="Start Date"
                  type="date"
                  value={filters.fromDate}
                  onChange={(e) => handleFilterChange('fromDate', e.target.value)}
                  InputLabelProps={{ shrink: true }}
                  fullWidth
                />
                <TextField
                  label="End Date"
                  type="date"
                  value={filters.toDate}
                  onChange={(e) => handleFilterChange('toDate', e.target.value)}
                  InputLabelProps={{ shrink: true }}
                  fullWidth
                />
              </Box>
            )}

            {showClass && (
              <FormControl fullWidth>
                <InputLabel>Class</InputLabel>
                <Select
                  value={filters.classRoomId}
                  label="Class"
                  onChange={(e) => handleFilterChange('classRoomId', e.target.value)}
                >
                  <MenuItem value="">
                    <em>All Classes</em>
                  </MenuItem>
                  {classes.map((cls) => (
                    <MenuItem key={cls.id} value={cls.id}>
                      {cls.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}

            {showSection && (
              <FormControl fullWidth>
                <InputLabel>Section</InputLabel>
                <Select
                  value={filters.sectionId}
                  label="Section"
                  onChange={(e) => handleFilterChange('sectionId', e.target.value)}
                  disabled={!filters.classRoomId || sections.length === 0}
                >
                  <MenuItem value="">
                    <em>All Sections</em>
                  </MenuItem>
                  {sections.map((section) => (
                    <MenuItem key={section.id} value={section.id}>
                      {section.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}

            {showExam && (
              <FormControl fullWidth>
                <InputLabel>Exam</InputLabel>
                <Select
                  value={filters.examId}
                  label="Exam"
                  onChange={(e) => handleFilterChange('examId', e.target.value)}
                >
                  <MenuItem value="">
                    <em>Select Exam</em>
                  </MenuItem>
                  {exams.map((exam) => (
                    <MenuItem key={exam.id} value={exam.id}>
                      {exam.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            )}
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2.5 }}>
          <Button onClick={handleCloseDialog} variant="outlined" disabled={generating}>
            Cancel
          </Button>
          <Button
            onClick={handleGenerate}
            variant="contained"
            startIcon={<DownloadIcon />}
            disabled={generating}
          >
            {generating ? 'Generating...' : 'Generate Report'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
