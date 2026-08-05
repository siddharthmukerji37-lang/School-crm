import React, { useState } from 'react';
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
import toast from 'react-hot-toast';

const reportTypes = [
  {
    id: 'student',
    title: 'Student Report',
    description: 'Generate reports on student enrollment, demographics, and academic performance.',
    icon: <SchoolIcon />,
    color: '#1976d2',
    filters: ['class', 'dateRange'],
  },
  {
    id: 'attendance',
    title: 'Attendance Report',
    description: 'View attendance records, trends, and summary statistics.',
    icon: <EventAvailableIcon />,
    color: '#388e3c',
    filters: ['class', 'dateRange'],
  },
  {
    id: 'fee',
    title: 'Fee Report',
    description: 'Analyze fee collection, outstanding payments, and financial summaries.',
    icon: <AttachMoneyIcon />,
    color: '#f57c00',
    filters: ['dateRange', 'feeType'],
  },
  {
    id: 'exam',
    title: 'Exam Report',
    description: 'View examination results, grades, and performance analysis.',
    icon: <QuizIcon />,
    color: '#7b1fa2',
    filters: ['class', 'examType'],
  },
  {
    id: 'employee',
    title: 'Employee Report',
    description: 'Generate reports on staff, departments, and employment details.',
    icon: <PeopleIcon />,
    color: '#00838f',
    filters: ['department', 'dateRange'],
  },
  {
    id: 'inventory',
    title: 'Inventory Report',
    description: 'Track inventory levels, stock movements, and asset valuation.',
    icon: <InventoryIcon />,
    color: '#5d4037',
    filters: ['category'],
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
    startDate: '',
    endDate: '',
    class: '',
    department: '',
    feeType: '',
    examType: '',
    category: '',
  });
  const [generating, setGenerating] = useState(false);

  const handleOpenDialog = (report) => {
    setSelectedReport(report);
    setFormat('pdf');
    setFilters({
      startDate: '',
      endDate: '',
      class: '',
      department: '',
      feeType: '',
      examType: '',
      category: '',
    });
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
    setGenerating(true);
    try {
      const params = {
        format,
        ...filters,
      };
      Object.keys(params).forEach((key) => {
        if (!params[key]) delete params[key];
      });

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

  const showDateRange = selectedReport?.filters?.includes('dateRange');
  const showClass = selectedReport?.filters?.includes('class');

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
                  value={filters.startDate}
                  onChange={(e) => handleFilterChange('startDate', e.target.value)}
                  InputLabelProps={{ shrink: true }}
                  fullWidth
                />
                <TextField
                  label="End Date"
                  type="date"
                  value={filters.endDate}
                  onChange={(e) => handleFilterChange('endDate', e.target.value)}
                  InputLabelProps={{ shrink: true }}
                  fullWidth
                />
              </Box>
            )}

            {showClass && (
              <TextField
                label="Class"
                value={filters.class}
                onChange={(e) => handleFilterChange('class', e.target.value)}
                fullWidth
                placeholder="e.g. 10A"
              />
            )}

            {selectedReport?.filters?.includes('department') && (
              <TextField
                label="Department"
                value={filters.department}
                onChange={(e) => handleFilterChange('department', e.target.value)}
                fullWidth
              />
            )}

            {selectedReport?.filters?.includes('feeType') && (
              <FormControl fullWidth>
                <InputLabel>Fee Type</InputLabel>
                <Select
                  value={filters.feeType}
                  label="Fee Type"
                  onChange={(e) => handleFilterChange('feeType', e.target.value)}
                >
                  <MenuItem value="tuition">Tuition</MenuItem>
                  <MenuItem value="transport">Transport</MenuItem>
                  <MenuItem value="hostel">Hostel</MenuItem>
                  <MenuItem value="exam">Exam</MenuItem>
                  <MenuItem value="other">Other</MenuItem>
                </Select>
              </FormControl>
            )}

            {selectedReport?.filters?.includes('examType') && (
              <TextField
                label="Exam Type"
                value={filters.examType}
                onChange={(e) => handleFilterChange('examType', e.target.value)}
                fullWidth
                placeholder="e.g. Mid-term, Final"
              />
            )}

            {selectedReport?.filters?.includes('category') && (
              <TextField
                label="Category"
                value={filters.category}
                onChange={(e) => handleFilterChange('category', e.target.value)}
                fullWidth
              />
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
