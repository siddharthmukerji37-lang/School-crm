import { useState, useEffect } from 'react';
import {
  Box, Paper, Typography, Button, TextField, Grid, Switch, FormControlLabel,
  CircularProgress, Alert, Card, CardContent, Divider, Select, MenuItem,
  FormControl, InputLabel,
} from '@mui/material';
import { Settings as SettingsIcon, Save as SaveIcon } from '@mui/icons-material';
import toast from 'react-hot-toast';
import payrollService from '../../services/payrollService';

const deductionTypes = [
  { value: 1, label: 'Fixed Amount' },
  { value: 2, label: 'Percentage' },
  { value: 3, label: 'Per Day' },
];

export default function AdminPayrollSettingsPage() {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [settings, setSettings] = useState({
    allowedLateCount: 6,
    lateDeductionEnabled: true,
    lateDeductionType: 1,
    lateDeductionAmount: 500,
    payrollDivisor: 30,
    requireAccountApproval: true,
  });

  useEffect(() => {
    fetchSettings();
  }, []);

  const fetchSettings = async () => {
    try {
      const res = await payrollService.getSettings();
      const data = res.data?.data ?? res.data;
      if (data) {
        setSettings({
          allowedLateCount: data.allowedLateCount ?? 6,
          lateDeductionEnabled: data.lateDeductionEnabled ?? true,
          lateDeductionType: data.lateDeductionType ?? 1,
          lateDeductionAmount: data.lateDeductionAmount ?? 500,
          payrollDivisor: data.payrollDivisor ?? 30,
          requireAccountApproval: data.requireAccountApproval ?? true,
        });
      }
    } catch (err) {
      toast.error('Failed to load settings');
    } finally {
      setLoading(false);
    }
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      await payrollService.saveSettings(settings);
      toast.success('Payroll settings saved');
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to save settings');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">
          <SettingsIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
          Payroll Settings
        </Typography>
        <Button
          variant="contained"
          startIcon={<SaveIcon />}
          onClick={handleSave}
          disabled={saving}
        >
          {saving ? 'Saving...' : 'Save Settings'}
        </Button>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>Late Attendance Rules</Typography>
              <Divider sx={{ mb: 2 }} />
              <FormControlLabel
                control={
                  <Switch
                    checked={settings.lateDeductionEnabled}
                    onChange={(e) => setSettings({ ...settings, lateDeductionEnabled: e.target.checked })}
                  />
                }
                label="Enable Late Deduction"
              />
              <TextField
                fullWidth
                type="number"
                label="Allowed Late Count (Free)"
                value={settings.allowedLateCount}
                onChange={(e) => setSettings({ ...settings, allowedLateCount: parseInt(e.target.value) || 0 })}
                sx={{ mt: 2 }}
                helperText="First N lates per month are free"
              />
              <FormControl fullWidth sx={{ mt: 2 }}>
                <InputLabel>Deduction Type</InputLabel>
                <Select
                  value={settings.lateDeductionType}
                  label="Deduction Type"
                  onChange={(e) => setSettings({ ...settings, lateDeductionType: parseInt(e.target.value) })}
                >
                  {deductionTypes.map((dt) => (
                    <MenuItem key={dt.value} value={dt.value}>{dt.label}</MenuItem>
                  ))}
                </Select>
              </FormControl>
              <TextField
                fullWidth
                type="number"
                label="Deduction Amount (per late)"
                value={settings.lateDeductionAmount}
                onChange={(e) => setSettings({ ...settings, lateDeductionAmount: parseFloat(e.target.value) || 0 })}
                sx={{ mt: 2 }}
              />
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>General Settings</Typography>
              <Divider sx={{ mb: 2 }} />
              <TextField
                fullWidth
                type="number"
                label="Payroll Divisor (days per month)"
                value={settings.payrollDivisor}
                onChange={(e) => setSettings({ ...settings, payrollDivisor: parseInt(e.target.value) || 30 })}
                helperText="Number used to calculate daily salary (e.g., 30 days)"
              />
              <FormControlLabel
                control={
                  <Switch
                    checked={settings.requireAccountApproval}
                    onChange={(e) => setSettings({ ...settings, requireAccountApproval: e.target.checked })}
                  />
                }
                label="Require Account Approval for Payroll"
                sx={{ mt: 2 }}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
