import { useState, useEffect } from 'react';
import {
  Box, Paper, Typography, Table, TableHead, TableBody, TableRow, TableCell,
  Chip, CircularProgress, Alert, Grid, Card, CardContent, Divider, Button,
  Dialog, DialogTitle, DialogContent, DialogActions,
} from '@mui/material';
import { AccountBalance as AccountIcon, Receipt as PayslipIcon } from '@mui/icons-material';
import toast from 'react-hot-toast';
import payrollService from '../../services/payrollService';

const months = [
  '', 'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

const statusColors = {
  1: 'default',
  2: 'info',
  3: 'warning',
  4: 'primary',
  5: 'secondary',
  6: 'success',
  7: 'error',
};

export default function MyPayrollPage() {
  const [loading, setLoading] = useState(true);
  const [profile, setProfile] = useState(null);
  const [payrolls, setPayrolls] = useState([]);
  const [payslips, setPayslips] = useState([]);
  const [detailDialog, setDetailDialog] = useState(null);
  const [detailPayroll, setDetailPayroll] = useState(null);
  const [payslipDialog, setPayslipDialog] = useState(null);
  const [payslipDetail, setPayslipDetail] = useState(null);

  useEffect(() => { fetchData(); }, []);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [profRes, payRes, psRes] = await Promise.all([
        payrollService.getMySalaryProfile(),
        payrollService.getMyPayrolls(),
        payrollService.getMyPayslips(),
      ]);
      const profData = profRes.data?.data ?? profRes.data;
      setProfile(profData && profData.id ? profData : null);
      const payData = payRes.data?.data ?? payRes.data ?? [];
      setPayrolls(Array.isArray(payData) ? payData : []);
      const psData = psRes.data?.data ?? psRes.data ?? [];
      setPayslips(Array.isArray(psData) ? psData : []);
    } catch (err) {
      toast.error('Failed to load payroll data');
    } finally {
      setLoading(false);
    }
  };

  const openDetail = async (p) => {
    setDetailPayroll(p);
    setDetailDialog(true);
  };

  const viewPayslip = async (p) => {
    try {
      const res = await payrollService.getMyPayslip(p.payrollId);
      const data = res.data?.data ?? res.data;
      setPayslipDetail(data);
      setPayslipDialog(true);
    } catch (err) {
      toast.error('No payslip available');
    }
  };

  const formatCurrency = (v) => {
    const num = parseFloat(v) || 0;
    return num.toLocaleString('en-IN', { style: 'currency', currency: 'INR' });
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
      <Typography variant="h4" mb={3}>
        <AccountIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
        My Payroll
      </Typography>

      {profile && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>Salary Profile</Typography>
            <Divider sx={{ mb: 2 }} />
            <Grid container spacing={2}>
              <Grid item xs={12} sm={3}>
                <Typography variant="subtitle2">Basic Salary</Typography>
                <Typography fontWeight={600}>{formatCurrency(profile.basicSalary)}</Typography>
              </Grid>
              <Grid item xs={12} sm={3}>
                <Typography variant="subtitle2">Allowances</Typography>
                <Typography>{formatCurrency(profile.allowances)}</Typography>
              </Grid>
              <Grid item xs={12} sm={3}>
                <Typography variant="subtitle2">Gross Salary</Typography>
                <Typography fontWeight={600}>{formatCurrency(profile.grossSalary)}</Typography>
              </Grid>
              <Grid item xs={12} sm={3}>
                <Typography variant="subtitle2">Bank</Typography>
                <Typography>{profile.bankName || 'N/A'}</Typography>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      )}

      {!profile && (
        <Alert severity="info" sx={{ mb: 3 }}>No salary profile configured. Please contact admin.</Alert>
      )}

      <Typography variant="h6" mb={2}>Payroll History</Typography>
      <Paper sx={{ width: '100%', overflow: 'auto', mb: 3 }}>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell>Period</TableCell>
              <TableCell align="right">Gross</TableCell>
              <TableCell align="right">Deductions</TableCell>
              <TableCell align="right">Net Pay</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="center">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {payrolls.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Alert severity="info">No payroll records yet.</Alert>
                </TableCell>
              </TableRow>
            ) : (
              payrolls.map((p) => (
                <TableRow key={p.id} hover sx={{ cursor: 'pointer' }} onClick={() => openDetail(p)}>
                  <TableCell>{months[p.payrollMonth]} {p.payrollYear}</TableCell>
                  <TableCell align="right">{formatCurrency(p.grossSalary)}</TableCell>
                  <TableCell align="right" color="error.main">{formatCurrency(p.totalDeductions)}</TableCell>
                  <TableCell align="right" fontWeight={600}>{formatCurrency(p.netSalary)}</TableCell>
                  <TableCell>
                    <Chip label={p.statusName || 'Unknown'} size="small" color={statusColors[p.status] || 'default'} />
                  </TableCell>
                  <TableCell align="center" onClick={(e) => e.stopPropagation()}>
                    {p.status === 6 && (
                      <Button size="small" startIcon={<PayslipIcon />} onClick={() => viewPayslip(p)}>
                        Payslip
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </Paper>

      {payslips.length > 0 && (
        <>
          <Typography variant="h6" mb={2}>My Payslips</Typography>
          <Paper sx={{ width: '100%', overflow: 'auto' }}>
            <Table stickyHeader>
              <TableHead>
                <TableRow>
                  <TableCell>Payslip Number</TableCell>
                  <TableCell>Period</TableCell>
                  <TableCell align="right">Gross</TableCell>
                  <TableCell align="right">Deductions</TableCell>
                  <TableCell align="right">Net Pay</TableCell>
                  <TableCell>Generated At</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {payslips.map((ps) => (
                  <TableRow key={ps.id} hover>
                    <TableCell><Chip label={ps.payslipNumber} size="small" /></TableCell>
                    <TableCell>{months[ps.payrollMonth]} {ps.payrollYear}</TableCell>
                    <TableCell align="right">{formatCurrency(ps.grossSalary)}</TableCell>
                    <TableCell align="right">{formatCurrency(ps.totalDeductions)}</TableCell>
                    <TableCell align="right" fontWeight={600}>{formatCurrency(ps.netSalary)}</TableCell>
                    <TableCell>{ps.generatedAt ? new Date(ps.generatedAt).toLocaleDateString() : 'N/A'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </Paper>
        </>
      )}

      <Dialog open={!!detailDialog} onClose={() => setDetailDialog(null)} maxWidth="sm" fullWidth>
        <DialogTitle>Payroll Details</DialogTitle>
        <DialogContent>
          {detailPayroll && (
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Period</Typography>
                <Typography>{months[detailPayroll.payrollMonth]} {detailPayroll.payrollYear}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Status</Typography>
                <Chip label={detailPayroll.statusName || 'Unknown'} size="small" color={statusColors[detailPayroll.status] || 'default'} />
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
              <Grid item xs={12}><Divider /></Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Late Count</Typography>
                <Typography>{detailPayroll.lateCount || 0} (allowed: {detailPayroll.allowedLateCount || 0})</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Late Deduction</Typography>
                <Typography color="error.main">{formatCurrency(detailPayroll.lateDeduction)}</Typography>
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
                <Typography variant="h6" fontWeight={700}>Net Pay: {formatCurrency(detailPayroll.netSalary)}</Typography>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailDialog(null)}>Close</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={!!payslipDialog} onClose={() => setPayslipDialog(null)} maxWidth="sm" fullWidth>
        <DialogTitle>Payslip</DialogTitle>
        <DialogContent>
          {payslipDetail && (
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Payslip Number</Typography>
                <Typography fontWeight={600}>{payslipDetail.payslipNumber}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Period</Typography>
                <Typography>{months[payslipDetail.payrollMonth]} {payslipDetail.payrollYear}</Typography>
              </Grid>
              <Grid item xs={12}><Divider /></Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Gross Salary</Typography>
                <Typography>{formatCurrency(payslipDetail.grossSalary)}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="subtitle2">Total Deductions</Typography>
                <Typography color="error.main">{formatCurrency(payslipDetail.totalDeductions)}</Typography>
              </Grid>
              <Grid item xs={12}>
                <Typography variant="h6" fontWeight={700}>Net Pay: {formatCurrency(payslipDetail.netSalary)}</Typography>
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle2">Generated</Typography>
                <Typography>{payslipDetail.generatedAt ? new Date(payslipDetail.generatedAt).toLocaleString() : 'N/A'}</Typography>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setPayslipDialog(null)}>Close</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
