import React, { useEffect, useState } from 'react';
import { Box, Paper, Typography, TextField, Button, Switch, FormControlLabel, MenuItem, Divider, CircularProgress, Alert } from '@mui/material';
import Grid from '@mui/material/Grid2';
import SaveIcon from '@mui/icons-material/Save';
import toast from 'react-hot-toast';
import attendancePolicyService from '../../services/attendancePolicyService';
import axiosInstance from '../../services/axiosInstance';

const DEDUCTION_TYPES = [
  { value: 1, label: 'Fixed Amount' },
  { value: 2, label: 'Percentage' },
  { value: 3, label: 'Per Day' },
];

export default function AttendancePolicyPage() {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [schoolId, setSchoolId] = useState(null);
  const [summaries, setSummaries] = useState([]);
  const [summariesLoading, setSummariesLoading] = useState(false);
  const [policy, setPolicy] = useState({
    allowedLateArrivals: 6,
    salaryDeductionEnabled: true,
    deductionType: 1,
    deductionAmount: 0,
    requireAdminApproval: true,
    schoolStartTime: '09:30',
    schoolEndTime: '18:30',
  });

  const now = new Date();
  const [selectedMonth, setSelectedMonth] = useState(now.getMonth() + 1);
  const [selectedYear, setSelectedYear] = useState(now.getFullYear());

  useEffect(() => {
    const loadPolicy = async () => {
      try {
        const schoolsRes = await axiosInstance.get('/schools');
        const schools = schoolsRes.data.data?.items || schoolsRes.data.data || [];
        if (schools.length > 0) {
          const sid = schools[0].id;
          setSchoolId(sid);
          const res = await attendancePolicyService.getPolicy(sid);
          const p = res.data.data;
          if (p) {
            setPolicy({
              allowedLateArrivals: p.allowedLateArrivals,
              salaryDeductionEnabled: p.salaryDeductionEnabled,
              deductionType: p.deductionType,
              deductionAmount: p.deductionAmount,
              requireAdminApproval: p.requireAdminApproval,
              schoolStartTime: p.schoolStartTime ? p.schoolStartTime.substring(0, 5) : '09:30',
              schoolEndTime: p.schoolEndTime ? p.schoolEndTime.substring(0, 5) : '18:30',
            });
          }
        }
      } catch {
        toast.error('Failed to load policy');
      } finally {
        setLoading(false);
      }
    };
    loadPolicy();
  }, []);

  const loadSummaries = async () => {
    setSummariesLoading(true);
    try {
      const res = await attendancePolicyService.getMonthlySummaries({
        month: selectedMonth,
        year: selectedYear,
        page: 1,
        pageSize: 100,
      });
      setSummaries(res.data.data?.items || []);
    } catch {
      setSummaries([]);
    } finally {
      setSummariesLoading(false);
    }
  };

  useEffect(() => {
    if (schoolId) loadSummaries();
  }, [schoolId, selectedMonth, selectedYear]);

  const handleSave = async () => {
    if (!schoolId) return;
    setSaving(true);
    try {
      await attendancePolicyService.updatePolicy(schoolId, {
        ...policy,
        schoolStartTime: `${policy.schoolStartTime}:00`,
        schoolEndTime: `${policy.schoolEndTime}:00`,
      });
      toast.success('Policy updated successfully');
    } catch {
      toast.error('Failed to update policy');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} sx={{ mb: 3 }}>
        Attendance Policy
      </Typography>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          School Timing
        </Typography>
        <Divider sx={{ mb: 3 }} />
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <TextField
              fullWidth
              label="School Start Time"
              type="time"
              value={policy.schoolStartTime}
              onChange={(e) => setPolicy({ ...policy, schoolStartTime: e.target.value })}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <TextField
              fullWidth
              label="School End Time"
              type="time"
              value={policy.schoolEndTime}
              onChange={(e) => setPolicy({ ...policy, schoolEndTime: e.target.value })}
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Grid>
        </Grid>
      </Paper>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          Late Attendance Policy
        </Typography>
        <Divider sx={{ mb: 3 }} />
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <TextField
              fullWidth
              label="Allowed Late Arrivals Per Month"
              type="number"
              value={policy.allowedLateArrivals}
              onChange={(e) => setPolicy({ ...policy, allowedLateArrivals: parseInt(e.target.value) || 6 })}
              helperText="Default: 6. Late arrivals beyond this may trigger salary deduction."
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <FormControlLabel
              control={
                <Switch
                  checked={policy.salaryDeductionEnabled}
                  onChange={(e) => setPolicy({ ...policy, salaryDeductionEnabled: e.target.checked })}
                />
              }
              label="Salary Deduction Enabled"
            />
          </Grid>
          {policy.salaryDeductionEnabled && (
            <>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField
                  fullWidth
                  select
                  label="Deduction Type"
                  value={policy.deductionType}
                  onChange={(e) => setPolicy({ ...policy, deductionType: parseInt(e.target.value) })}
                >
                  {DEDUCTION_TYPES.map((dt) => (
                    <MenuItem key={dt.value} value={dt.value}>
                      {dt.label}
                    </MenuItem>
                  ))}
                </TextField>
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField
                  fullWidth
                  label="Deduction Amount"
                  type="number"
                  value={policy.deductionAmount}
                  onChange={(e) => setPolicy({ ...policy, deductionAmount: parseFloat(e.target.value) || 0 })}
                  helperText={
                    policy.deductionType === 1 ? 'Fixed amount per deduction' :
                    policy.deductionType === 2 ? 'Percentage of salary' :
                    'Amount per day deduction'
                  }
                />
              </Grid>
              <Grid size={{ xs: 12 }}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={policy.requireAdminApproval}
                      onChange={(e) => setPolicy({ ...policy, requireAdminApproval: e.target.checked })}
                    />
                  }
                  label="Require Admin Approval for Deductions"
                />
              </Grid>
            </>
          )}
        </Grid>
      </Paper>

      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 3 }}>
        <Button
          variant="contained"
          startIcon={saving ? <CircularProgress size={18} /> : <SaveIcon />}
          onClick={handleSave}
          disabled={saving}
        >
          Save Policy
        </Button>
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          Monthly Late Summary
        </Typography>
        <Divider sx={{ mb: 3 }} />
        <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
          <TextField
            select
            size="small"
            label="Month"
            value={selectedMonth}
            onChange={(e) => setSelectedMonth(parseInt(e.target.value))}
            sx={{ minWidth: 120 }}
          >
            {months.map((m, i) => (
              <MenuItem key={i + 1} value={i + 1}>{m}</MenuItem>
            ))}
          </TextField>
          <TextField
            size="small"
            label="Year"
            type="number"
            value={selectedYear}
            onChange={(e) => setSelectedYear(parseInt(e.target.value))}
            sx={{ minWidth: 100 }}
          />
        </Box>
        {summariesLoading ? (
          <CircularProgress size={24} />
        ) : summaries.length === 0 ? (
          <Alert severity="info">No late attendance records for this month.</Alert>
        ) : (
          <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse' }}>
            <Box component="thead">
              <Box component="tr" sx={{ borderBottom: '1px solid', borderColor: 'divider' }}>
                <Box component="th" sx={{ textAlign: 'left', py: 1, px: 1, fontWeight: 600 }}>Name</Box>
                <Box component="th" sx={{ textAlign: 'center', py: 1, px: 1, fontWeight: 600 }}>Late Count</Box>
                <Box component="th" sx={{ textAlign: 'center', py: 1, px: 1, fontWeight: 600 }}>Allowed</Box>
                <Box component="th" sx={{ textAlign: 'center', py: 1, px: 1, fontWeight: 600 }}>Status</Box>
                <Box component="th" sx={{ textAlign: 'center', py: 1, px: 1, fontWeight: 600 }}>Deductions</Box>
              </Box>
            </Box>
            <Box component="tbody">
              {summaries.map((s) => (
                <Box component="tr" key={s.id} sx={{ borderBottom: '1px solid', borderColor: 'divider' }}>
                  <Box component="td" sx={{ py: 1, px: 1 }}>{s.userName}</Box>
                  <Box component="td" sx={{ py: 1, px: 1, textAlign: 'center' }}>{s.totalLateCount}</Box>
                  <Box component="td" sx={{ py: 1, px: 1, textAlign: 'center' }}>{s.allowedLateCount}</Box>
                  <Box component="td" sx={{ py: 1, px: 1, textAlign: 'center' }}>
                    <span style={{ color: s.policyExceeded ? '#f44336' : '#4caf50', fontWeight: 600 }}>
                      {s.policyExceeded ? 'Exceeded' : 'Within Limit'}
                    </span>
                  </Box>
                  <Box component="td" sx={{ py: 1, px: 1, textAlign: 'center' }}>{s.salaryDeductionCount}</Box>
                </Box>
              ))}
            </Box>
          </Box>
        )}
      </Paper>
    </Box>
  );
}
