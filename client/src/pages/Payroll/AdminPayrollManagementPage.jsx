import { useState, useEffect } from 'react';
import {
  Box, Paper, Typography, Button, Table, TableHead, TableBody, TableRow, TableCell,
  Chip, IconButton, CircularProgress, Alert, Grid, Card, CardContent, Divider,
  TextField, MenuItem, Dialog, DialogTitle, DialogContent, DialogActions, Tooltip,
} from '@mui/material';
import {
  PlayArrow as GenerateIcon, CheckCircle as ApproveIcon, Payment as PayIcon,
  Receipt as PayslipIcon, Assessment as ReportIcon,
} from '@mui/icons-material';
import toast from 'react-hot-toast';
import payrollService from '../../services/payrollService';

const statusColors = {
  1: 'default',
  2: 'info',
  3: 'warning',
  4: 'primary',
  5: 'secondary',
  6: 'success',
  7: 'error',
};

const months = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

export default function AdminPayrollManagementPage() {
  const [loading, setLoading] = useState(true);
  const [payrolls, setPayrolls] = useState([]);
  const [report, setReport] = useState(null);
  const [selectedMonth, setSelectedMonth] = useState(new Date().getMonth() + 1);
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
  const [generating, setGenerating] = useState(false);
  const [detailDialog, setDetailDialog] = useState(null);
  const [detailPayroll, setDetailPayroll] = useState(null);

  useEffect(() => { fetchData(); }, [selectedMonth, selectedYear]);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [payRes, repRes] = await Promise.all([
        payrollService.getPayrolls(selectedMonth, selectedYear),
        payrollService.getReport(selectedMonth, selectedYear),
      ]);
      const pData = payRes.data?.data ?? payRes.data ?? [];
      setPayrolls(Array.isArray(pData) ? pData : []);
      const rData = repRes.data?.data ?? repRes.data ?? null;
      setReport(rData);
    } catch (err) {
      toast.error('Failed to load payroll data');
    } finally {
      setLoading(false);
    }
  };

  const handleGenerate = async () => {
    setGenerating(true);
    try {
      const res = await payrollService.generatePayroll({ month: selectedMonth, year: selectedYear });
      const data = res.data?.data ?? res.data ?? [];
      const count = Array.isArray(data) ? data.length : 0;
      toast.success(`Generated payroll for ${count} employees`);
      fetchData();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to generate payroll');
    } finally {
      setGenerating(false);
    }
  };

  const handleApprove = async (id) => {
    try {
      await payrollService.approvePayroll(id);
      toast.success('Payroll approved');
      fetchData();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to approve');
    }
  };

  const handleMarkPaid = async (id) => {
    try {
      await payrollService.markPaid(id);
      toast.success('Payroll marked as paid');
      fetchData();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to mark as paid');
    }
  };

  const handleGeneratePayslip = async (id) => {
    try {
      await payrollService.generatePayslip(id);
      toast.success('Payslip generated');
      fetchData();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to generate payslip');
    }
  };

  const openDetail = async (p) => {
    try {
      const res = await payrollService.getPayroll(p.id);
      const data = res.data?.data ?? res.data;
      setDetailPayroll(data);
      setDetailDialog(true);
    } catch (err) {
      toast.error('Failed to load payroll details');
    }
  };

  const formatCurrency = (v) => {
    const num = parseFloat(v) || 0;
    return num.toLocaleString('en-IN', { style: 'currency', currency: 'INR' });
  };

  return (
    <Box>
      <Typography variant="h4" mb={3}>
        <ReportIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
        Payroll Management
      </Typography>

      <Grid container spacing={3} mb={3}>
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Grid container spacing={2} alignItems="center">
                <Grid item xs={4}>
                  <TextField
                    fullWidth select size="small"
                    label="Month"
                    value={selectedMonth}
                    onChange={(e) => setSelectedMonth(parseInt(e.target.value))}
                  >
                    {months.map((m, i) => (
                      <MenuItem key={i} value={i + 1}>{m}</MenuItem>
                    ))}
                  </TextField>
                </Grid>
                <Grid item xs={3}>
                  <TextField
                    fullWidth type="number" size="small"
                    label="Year"
                    value={selectedYear}
                    onChange={(e) => setSelectedYear(parseInt(e.target.value))}
                  />
                </Grid>
                <Grid item xs={5}>
                  <Button
                    variant="contained"
                    startIcon={<GenerateIcon />}
                    onClick={handleGenerate}
                    disabled={generating}
                    fullWidth
                  >
                    {generating ? 'Generating...' : 'Generate Payroll'}
                  </Button>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>
        {report && (
          <Grid item xs={12} md={4}>
            <Card>
              <CardContent>
                <Typography variant="subtitle2" gutterBottom>Summary</Typography>
                <Divider sx={{ mb: 1 }} />
                <Typography variant="body2">Employees: {report.totalEmployees}</Typography>
                <Typography variant="body2">Approved: {report.payrollApproved}</Typography>
                <Typography variant="body2" fontWeight={600}>
                  Net Payroll: {formatCurrency(report.totalNetPayroll)}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>

      <Paper sx={{ width: '100%', overflow: 'auto' }}>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Type</TableCell>
              <TableCell align="right">Gross</TableCell>
              <TableCell align="right">Deductions</TableCell>
              <TableCell align="right">Net</TableCell>
              <TableCell align="center">Lates</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="center">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={8} align="center"><CircularProgress /></TableCell>
              </TableRow>
            ) : payrolls.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  <Alert severity="info">No payroll records for this month. Click "Generate Payroll" to create them.</Alert>
                </TableCell>
              </TableRow>
            ) : (
              payrolls.map((p) => (
                <TableRow key={p.id} hover sx={{ cursor: 'pointer' }} onClick={() => openDetail(p)}>
                  <TableCell>
                    <Typography variant="body2" fontWeight={600}>{p.userName || 'N/A'}</Typography>
                  </TableCell>
                  <TableCell><Chip label={p.userType || 'N/A'} size="small" /></TableCell>
                  <TableCell align="right">{formatCurrency(p.grossSalary)}</TableCell>
                  <TableCell align="right" color="error.main">{formatCurrency(p.totalDeductions)}</TableCell>
                  <TableCell align="right" fontWeight={600}>{formatCurrency(p.netSalary)}</TableCell>
                  <TableCell align="center">{p.lateCount || 0}</TableCell>
                  <TableCell>
                    <Chip label={p.statusName || 'Unknown'} size="small" color={statusColors[p.status] || 'default'} />
                  </TableCell>
                  <TableCell align="center" onClick={(e) => e.stopPropagation()}>
                    {(p.status === 2 || p.status === 3) && (
                      <Tooltip title="Approve">
                        <IconButton size="small" onClick={() => handleApprove(p.id)} color="primary">
                          <ApproveIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {(p.status === 4) && (
                      <Tooltip title="Generate Payslip">
                        <IconButton size="small" onClick={() => handleGeneratePayslip(p.id)} color="secondary">
                          <PayslipIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                    {(p.status === 4 || p.status === 5) && (
                      <Tooltip title="Mark as Paid">
                        <IconButton size="small" onClick={() => handleMarkPaid(p.id)} color="success">
                          <PayIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    )}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </Paper>

      <Dialog open={!!detailDialog} onClose={() => setDetailDialog(null)} maxWidth="md" fullWidth>
        <DialogTitle>Payroll Details</DialogTitle>
        <DialogContent>
          {detailPayroll && (
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Employee</Typography>
                <Typography>{detailPayroll.userName || 'N/A'}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Period</Typography>
                <Typography>{months[(detailPayroll.payrollMonth || 1) - 1]} {detailPayroll.payrollYear}</Typography>
              </Grid>
              <Grid item xs={12}><Divider /></Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Basic Salary</Typography>
                <Typography>{formatCurrency(detailPayroll.basicSalary)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Allowances</Typography>
                <Typography>{formatCurrency(detailPayroll.totalAllowances)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Gross Salary</Typography>
                <Typography fontWeight={600}>{formatCurrency(detailPayroll.grossSalary)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Daily Salary</Typography>
                <Typography>{formatCurrency(detailPayroll.dailySalary)} (÷ {detailPayroll.payrollDivisor} days)</Typography>
              </Grid>
              <Grid item xs={12}><Divider /></Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Late Count</Typography>
                <Typography>{detailPayroll.lateCount} (allowed: {detailPayroll.allowedLateCount})</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Late Deduction</Typography>
                <Typography color="error.main">{formatCurrency(detailPayroll.lateDeduction)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Paid Leave Days</Typography>
                <Typography>{detailPayroll.paidLeaveDays}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Unpaid Leave Days</Typography>
                <Typography>{detailPayroll.unpaidLeaveDays}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Unpaid Leave Deduction</Typography>
                <Typography color="error.main">{formatCurrency(detailPayroll.unpaidLeaveDeduction)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Total Deductions</Typography>
                <Typography color="error.main" fontWeight={600}>{formatCurrency(detailPayroll.totalDeductions)}</Typography>
              </Grid>
              <Grid item xs={12}><Divider /></Grid>
              <Grid item xs={12}>
                <Typography variant="h5" fontWeight={700}>Net Salary: {formatCurrency(detailPayroll.netSalary)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Status</Typography>
                <Chip label={detailPayroll.statusName || 'Unknown'} color={statusColors[detailPayroll.status] || 'default'} />
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Calculated At</Typography>
                <Typography>{detailPayroll.calculatedAt ? new Date(detailPayroll.calculatedAt).toLocaleString() : 'N/A'}</Typography>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailDialog(null)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
