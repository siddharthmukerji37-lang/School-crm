import React, { useEffect, useState } from 'react';
import {
  Box,
  Tabs,
  Tab,
  Paper,
  TextField,
  Button,
  Typography,
  Grid,
  Switch,
  Divider,
  CircularProgress,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
} from '@mui/material';
import SaveIcon from '@mui/icons-material/Save';
import DownloadIcon from '@mui/icons-material/Download';
import axiosInstance from '../../services/axiosInstance';
import PageHeader from '../../components/common/PageHeader';
import toast from 'react-hot-toast';

function TabPanel({ children, value, index, ...other }) {
  return (
    <div role="tabpanel" hidden={value !== index} {...other}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  );
}

function SchoolProfileTab() {
  const [school, setSchool] = useState({
    id: '',
    name: '',
    address: '',
    phone: '',
    email: '',
    website: '',
    logo: '',
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    const fetchSchool = async () => {
      try {
        const response = await axiosInstance.get('/schools');
        const items = response.data.data?.items || response.data.data || [];
        const first = Array.isArray(items) ? items[0] : items;
        if (first) {
          setSchool({
            id: first.id || '',
            name: first.name || '',
            address: first.address || '',
            phone: first.phone || '',
            email: first.email || '',
            website: first.website || '',
            logo: first.logo || '',
          });
        }
      } catch (error) {
        toast.error('Failed to load school information');
      } finally {
        setLoading(false);
      }
    };
    fetchSchool();
  }, []);

  const handleChange = (field, value) => {
    setSchool((prev) => ({ ...prev, [field]: value }));
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await axiosInstance.put(`/schools/${school.id}`, school);
      toast.success('School information updated successfully');
    } catch (error) {
      toast.error(error.message || 'Failed to update school information');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Paper sx={{ p: 3, borderRadius: 2 }}>
      <Typography variant="h6" fontWeight={600} mb={3}>
        School Information
      </Typography>
      <Grid container spacing={3}>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="School Name"
            value={school.name}
            onChange={(e) => handleChange('name', e.target.value)}
            fullWidth
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="Phone"
            value={school.phone}
            onChange={(e) => handleChange('phone', e.target.value)}
            fullWidth
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            label="Address"
            value={school.address}
            onChange={(e) => handleChange('address', e.target.value)}
            fullWidth
            multiline
            rows={2}
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="Email"
            value={school.email}
            onChange={(e) => handleChange('email', e.target.value)}
            fullWidth
          />
        </Grid>
        <Grid size={{ xs: 12, sm: 6 }}>
          <TextField
            label="Website"
            value={school.website}
            onChange={(e) => handleChange('website', e.target.value)}
            fullWidth
          />
        </Grid>
        <Grid size={{ xs: 12 }}>
          <TextField
            label="Logo URL"
            value={school.logo}
            onChange={(e) => handleChange('logo', e.target.value)}
            fullWidth
          />
        </Grid>
      </Grid>
      <Box sx={{ mt: 3, display: 'flex', justifyContent: 'flex-end' }}>
        <Button
          variant="contained"
          startIcon={<SaveIcon />}
          onClick={handleSave}
          disabled={saving}
        >
          {saving ? 'Saving...' : 'Save Changes'}
        </Button>
      </Box>
    </Paper>
  );
}

function AcademicYearTab() {
  const [years, setYears] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchYears = async () => {
      try {
        const response = await axiosInstance.get('/academic-years');
        const data = response.data.data || response.data;
        setYears(Array.isArray(data) ? data : data.items || []);
      } catch (error) {
        toast.error('Failed to load academic years');
      } finally {
        setLoading(false);
      }
    };
    fetchYears();
  }, []);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Paper sx={{ p: 3, borderRadius: 2 }}>
      <Typography variant="h6" fontWeight={600} mb={3}>
        Academic Years
      </Typography>
      {years.length === 0 ? (
        <Typography color="text.secondary">No academic years found.</Typography>
      ) : (
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Name</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Start Date</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>End Date</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {years.map((year) => (
                <TableRow key={year.id}>
                  <TableCell>{year.name}</TableCell>
                  <TableCell>
                    {year.startDate ? new Date(year.startDate).toLocaleDateString() : '-'}
                  </TableCell>
                  <TableCell>
                    {year.endDate ? new Date(year.endDate).toLocaleDateString() : '-'}
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={year.isActive ? 'Active' : 'Inactive'}
                      color={year.isActive ? 'success' : 'default'}
                      size="small"
                      variant="outlined"
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Paper>
  );
}

function BackupTab() {
  const [exporting, setExporting] = useState(false);

  const handleExport = async () => {
    setExporting(true);
    try {
      const token = localStorage.getItem('token');
      const baseUrl = import.meta.env.VITE_API_BASE_URL || '/api';
      const response = await fetch(`${baseUrl}/backup/export`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Export failed');
      }

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `school_backup_${new Date().toISOString().slice(0, 10)}.json`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);

      toast.success('Data exported successfully');
    } catch (error) {
      toast.error(error.message || 'Failed to export data');
    } finally {
      setExporting(false);
    }
  };

  return (
    <Paper sx={{ p: 3, borderRadius: 2 }}>
      <Typography variant="h6" fontWeight={600} mb={1}>
        Backup & Export
      </Typography>
      <Typography variant="body2" color="text.secondary" mb={3}>
        Export all school data as a JSON backup file. This can be used to restore data in case of
        system failure or migration.
      </Typography>
      <Divider sx={{ my: 2 }} />
      <Stack spacing={2}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box>
            <Typography variant="subtitle1" fontWeight={500}>
              Full Data Export
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Downloads all school data including students, staff, finances, and records.
            </Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<DownloadIcon />}
            onClick={handleExport}
            disabled={exporting}
          >
            {exporting ? 'Exporting...' : 'Export Data'}
          </Button>
        </Box>
      </Stack>
    </Paper>
  );
}

const tabLabels = ['School Profile', 'Academic Year', 'Backup'];

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState(0);

  return (
    <Box>
      <PageHeader title="Settings" subtitle="Manage school configuration and preferences" />

      <Paper sx={{ borderRadius: 2 }}>
        <Tabs
          value={activeTab}
          onChange={(_, newValue) => setActiveTab(newValue)}
          sx={{ borderBottom: 1, borderColor: 'divider', px: 2 }}
        >
          {tabLabels.map((label) => (
            <Tab key={label} label={label} />
          ))}
        </Tabs>

        <TabPanel value={activeTab} index={0}>
          <SchoolProfileTab />
        </TabPanel>
        <TabPanel value={activeTab} index={1}>
          <AcademicYearTab />
        </TabPanel>
        <TabPanel value={activeTab} index={2}>
          <BackupTab />
        </TabPanel>
      </Paper>
    </Box>
  );
}
