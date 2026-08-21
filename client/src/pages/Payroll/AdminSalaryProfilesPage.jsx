import { useState, useEffect } from 'react';
import {
  Box, Paper, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableHead, TableBody, TableRow, TableCell, Chip, IconButton,
  CircularProgress, Grid, Switch, FormControlLabel, Alert,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, AttachMoney as MoneyIcon } from '@mui/icons-material';
import toast from 'react-hot-toast';
import payrollService from '../../services/payrollService';

const initialProfileState = {
  userId: '', basicSalary: '', allowances: '', effectiveFrom: '',
  payrollDivisor: '', isActive: true, bankName: '', bankAccountNumber: '', bankIFSC: '',
};

export default function AdminSalaryProfilesPage() {
  const [profiles, setProfiles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingProfile, setEditingProfile] = useState(null);
  const [form, setForm] = useState(initialProfileState);
  const [submitting, setSubmitting] = useState(false);
  const [users, setUsers] = useState([]);

  useEffect(() => { fetchProfiles(); }, []);

  const fetchProfiles = async () => {
    setLoading(true);
    try {
      const res = await payrollService.getAllSalaryProfiles();
      const data = res.data?.data ?? res.data ?? [];
      setProfiles(Array.isArray(data) ? data : []);
    } catch (err) {
      toast.error('Failed to load salary profiles');
    } finally {
      setLoading(false);
    }
  };

  const openDialog = (profile = null) => {
    setEditingProfile(profile);
    if (profile) {
      setForm({
        userId: profile.userId || '',
        basicSalary: profile.basicSalary || '',
        allowances: profile.allowances || '',
        effectiveFrom: profile.effectiveFrom ? profile.effectiveFrom.substring(0, 10) : '',
        payrollDivisor: profile.payrollDivisor || '',
        isActive: profile.isActive ?? true,
        bankName: profile.bankName || '',
        bankAccountNumber: profile.bankAccountNumber || '',
        bankIFSC: profile.bankIFSC || '',
      });
    } else {
      setForm(initialProfileState);
    }
    setDialogOpen(true);
  };

  const handleSave = async () => {
    setSubmitting(true);
    try {
      const payload = {
        userId: form.userId,
        basicSalary: parseFloat(form.basicSalary) || 0,
        allowances: parseFloat(form.allowances) || 0,
        effectiveFrom: form.effectiveFrom || new Date().toISOString(),
        payrollDivisor: form.payrollDivisor ? parseInt(form.payrollDivisor) : null,
        isActive: form.isActive,
        bankName: form.bankName || null,
        bankAccountNumber: form.bankAccountNumber || null,
        bankIFSC: form.bankIFSC || null,
      };
      if (editingProfile) {
        await payrollService.updateSalaryProfile(editingProfile.id, payload);
        toast.success('Profile updated');
      } else {
        await payrollService.createSalaryProfile(payload);
        toast.success('Profile created');
      }
      setDialogOpen(false);
      fetchProfiles();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to save profile');
    } finally {
      setSubmitting(false);
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
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">
          <MoneyIcon sx={{ mr: 1, verticalAlign: 'middle' }} />
          Salary Profiles
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />} onClick={() => openDialog()}>
          Add Salary Profile
        </Button>
      </Box>

      <Paper sx={{ width: '100%', overflow: 'auto' }}>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Type</TableCell>
              <TableCell align="right">Basic Salary</TableCell>
              <TableCell align="right">Allowances</TableCell>
              <TableCell align="right">Gross Salary</TableCell>
              <TableCell>Effective From</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="center">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {profiles.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  <Alert severity="info">No salary profiles configured yet.</Alert>
                </TableCell>
              </TableRow>
            ) : (
              profiles.map((p) => (
                <TableRow key={p.id} hover>
                  <TableCell>
                    <Typography variant="body2" fontWeight={600}>{p.userName || 'N/A'}</Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={p.userType || 'N/A'} size="small" color={p.userType === 'Teacher' ? 'primary' : 'secondary'} />
                  </TableCell>
                  <TableCell align="right">{formatCurrency(p.basicSalary)}</TableCell>
                  <TableCell align="right">{formatCurrency(p.allowances)}</TableCell>
                  <TableCell align="right" fontWeight={600}>{formatCurrency(p.grossSalary)}</TableCell>
                  <TableCell>{p.effectiveFrom ? new Date(p.effectiveFrom).toLocaleDateString() : 'N/A'}</TableCell>
                  <TableCell>
                    <Chip label={p.isActive ? 'Active' : 'Inactive'} size="small" color={p.isActive ? 'success' : 'default'} />
                  </TableCell>
                  <TableCell align="center">
                    <IconButton size="small" onClick={() => openDialog(p)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </Paper>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editingProfile ? 'Edit Salary Profile' : 'Add Salary Profile'}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            {!editingProfile && (
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="User ID"
                  value={form.userId}
                  onChange={(e) => setForm({ ...form, userId: e.target.value })}
                  required
                  helperText="Enter the User ID of the teacher or employee"
                />
              </Grid>
            )}
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="Basic Salary"
                value={form.basicSalary}
                onChange={(e) => setForm({ ...form, basicSalary: e.target.value })}
                required
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="Allowances"
                value={form.allowances}
                onChange={(e) => setForm({ ...form, allowances: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="date"
                label="Effective From"
                value={form.effectiveFrom}
                onChange={(e) => setForm({ ...form, effectiveFrom: e.target.value })}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                type="number"
                label="Custom Payroll Divisor"
                value={form.payrollDivisor}
                onChange={(e) => setForm({ ...form, payrollDivisor: e.target.value })}
                helperText="Leave empty to use default"
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField fullWidth label="Bank Name" value={form.bankName} onChange={(e) => setForm({ ...form, bankName: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField fullWidth label="Bank Account Number" value={form.bankAccountNumber} onChange={(e) => setForm({ ...form, bankAccountNumber: e.target.value })} />
            </Grid>
            <Grid item xs={12}>
              <TextField fullWidth label="IFSC Code" value={form.bankIFSC} onChange={(e) => setForm({ ...form, bankIFSC: e.target.value })} />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} />
                }
                label="Active"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={handleSave} disabled={submitting}>
            {submitting ? 'Saving...' : editingProfile ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
