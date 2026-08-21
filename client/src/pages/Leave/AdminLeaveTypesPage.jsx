import { useState, useEffect } from 'react';
import {
  Box, Paper, Typography, Button, TextField, Dialog, DialogTitle, DialogContent,
  DialogActions, Table, TableHead, TableBody, TableRow, TableCell, Chip, IconButton,
  CircularProgress, Alert, Grid, Tabs, Tab, Switch, FormControlLabel, FormControl,
  InputLabel, Select, MenuItem,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, ToggleOn as ToggleOnIcon, ToggleOff as ToggleOffIcon } from '@mui/icons-material';
import toast from 'react-hot-toast';
import leaveService from '../../services/leaveService';

const initialLeaveTypeState = { name: '', code: '', description: '', requiresApproval: true, requiresAttachment: false, isActive: true };
const initialCalendarState = { name: '', year: new Date().getFullYear(), startDate: '', endDate: '', isActive: true };
const initialConfigState = { leaveTypeId: '', totalDays: '', isPaid: true, applicableGender: 1, applicableUserType: 'Both', minimumDays: 1, maximumDays: 15, isActive: true };

const genderMap = { 1: 'Male', 2: 'Female', 3: 'Other', All: 1 };
const genderLabel = (v) => ({ 1: 'Male', 2: 'Female', 3: 'Other' }[v] || 'All');

export default function AdminLeaveTypesPage() {
  const [activeTab, setActiveTab] = useState(0);
  const [leaveTypes, setLeaveTypes] = useState([]);
  const [calendars, setCalendars] = useState([]);
  const [activeCalendar, setActiveCalendar] = useState(null);
  const [leaveConfigs, setLeaveConfigs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogType, setDialogType] = useState('');
  const [editingItem, setEditingItem] = useState(null);
  const [leaveTypeForm, setLeaveTypeForm] = useState(initialLeaveTypeState);
  const [calendarForm, setCalendarForm] = useState(initialCalendarState);
  const [configForm, setConfigForm] = useState(initialConfigState);
  const [submitting, setSubmitting] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    try {
      const [ltRes, calRes] = await Promise.all([
        leaveService.getLeaveTypes(),
        leaveService.getCalendars(),
      ]);
      const ltData = ltRes.data?.data ?? ltRes.data ?? [];
      const calData = calRes.data?.data ?? calRes.data ?? [];

      setLeaveTypes(Array.isArray(ltData) ? ltData : []);
      setCalendars(Array.isArray(calData) ? calData : []);

      const active = (Array.isArray(calData) ? calData : []).find((c) => c.isActive);
      setActiveCalendar(active || null);

      if (active) {
        const cfgRes = await leaveService.getLeaveConfigs(active.id);
        const cfgData = cfgRes.data?.data ?? cfgRes.data ?? [];
        setLeaveConfigs(Array.isArray(cfgData) ? cfgData : []);
      } else {
        setLeaveConfigs([]);
      }
    } catch (err) {
      toast.error('Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { fetchData(); }, []);

  const handleTabChange = (_, v) => setActiveTab(v);

  const openDialog = (type, item = null) => {
    setDialogType(type);
    setEditingItem(item);
    if (type === 'leave_type') {
      setLeaveTypeForm(item ? { ...item } : { ...initialLeaveTypeState });
    } else if (type === 'calendar') {
      if (item) {
        setCalendarForm({
          name: item.name || '',
          year: item.year || new Date().getFullYear(),
          startDate: item.startDate ? item.startDate.substring(0, 10) : '',
          endDate: item.endDate ? item.endDate.substring(0, 10) : '',
          isActive: item.isActive ?? true,
        });
      } else {
        setCalendarForm({ ...initialCalendarState });
      }
    } else if (type === 'config') {
      if (item) {
        setConfigForm({
          leaveTypeId: item.leaveTypeId || '',
          totalDays: item.totalDays || '',
          isPaid: item.isPaid ?? true,
          applicableGender: item.applicableGender ?? 1,
          applicableUserType: item.applicableUserType || 'Both',
          minimumDays: item.minimumDays || 1,
          maximumDays: item.maximumDays || 15,
          isActive: item.isActive ?? true,
        });
      } else {
        setConfigForm({ ...initialConfigState });
      }
    }
    setDialogOpen(true);
  };

  const closeDialog = () => { setDialogOpen(false); setEditingItem(null); };

  const handleSubmit = async () => {
    setSubmitting(true);
    try {
      if (dialogType === 'leave_type') {
        if (editingItem) {
          await leaveService.updateLeaveType(editingItem.id, leaveTypeForm);
          toast.success('Leave type updated');
        } else {
          await leaveService.createLeaveType(leaveTypeForm);
          toast.success('Leave type created');
        }
      } else if (dialogType === 'calendar') {
        if (editingItem) {
          await leaveService.updateCalendar(editingItem.id, calendarForm);
          toast.success('Calendar updated');
        } else {
          await leaveService.createCalendar(calendarForm);
          toast.success('Calendar created');
        }
      } else if (dialogType === 'config') {
        const payload = {
          leaveTypeId: configForm.leaveTypeId,
          totalDays: parseInt(configForm.totalDays) || 0,
          isPaid: configForm.isPaid,
          applicableGender: typeof configForm.applicableGender === 'string' ? (genderMap[configForm.applicableGender] || 1) : configForm.applicableGender,
          applicableUserType: configForm.applicableUserType === 'all' ? 'Both' : configForm.applicableUserType,
          minimumDays: parseInt(configForm.minimumDays) || 1,
          maximumDays: parseInt(configForm.maximumDays) || 30,
          isActive: configForm.isActive,
        };
        if (editingItem) {
          await leaveService.updateLeaveConfig(editingItem.id, payload);
          toast.success('Configuration updated');
        } else {
          await leaveService.createLeaveConfig(activeCalendar.id, payload);
          toast.success('Configuration created');
        }
      }
      closeDialog();
      fetchData();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Operation failed');
    } finally {
      setSubmitting(false);
    }
  };

  const handleToggleStatus = async (id, currentStatus) => {
    try {
      const lt = leaveTypes.find((t) => t.id === id);
      if (!lt) return;
      await leaveService.updateLeaveType(id, {
        name: lt.name,
        code: lt.code,
        description: lt.description || '',
        requiresApproval: lt.requiresApproval,
        requiresAttachment: lt.requiresAttachment,
        isActive: !currentStatus,
      });
      toast.success('Status updated');
      fetchData();
    } catch (err) {
      toast.error('Failed to update status');
    }
  };

  const handleInitializeBalances = async () => {
    if (!activeCalendar) { toast.error('No active calendar found'); return; }
    try {
      await leaveService.initializeBalances(activeCalendar.id);
      toast.success('Leave balances initialized');
    } catch (err) {
      toast.error('Failed to initialize balances');
    }
  };

  const handleToggleConfigActive = async (id, currentStatus) => {
    try {
      const cfg = leaveConfigs.find((c) => c.id === id);
      if (!cfg) return;
      await leaveService.updateLeaveConfig(id, {
        totalDays: cfg.totalDays,
        isPaid: cfg.isPaid,
        applicableGender: cfg.applicableGender,
        applicableUserType: cfg.applicableUserType,
        minimumDays: cfg.minimumDays,
        maximumDays: cfg.maximumDays,
        isActive: !currentStatus,
      });
      toast.success('Config status updated');
      fetchData();
    } catch (err) {
      toast.error('Failed to update config status');
    }
  };

  if (loading) {
    return <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px"><CircularProgress /></Box>;
  }

  return (
    <Box p={3}>
      <Typography variant="h4" gutterBottom>Leave Management</Typography>
      <Paper sx={{ mb: 3 }}>
        <Tabs value={activeTab} onChange={handleTabChange}>
          <Tab label="Leave Types" />
          <Tab label="Calendar & Leave Configuration" />
        </Tabs>
      </Paper>

      {activeTab === 0 && (
        <Box>
          <Grid container justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
            <Typography variant="h6">All Leave Types</Typography>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => openDialog('leave_type')}>Add Leave Type</Button>
          </Grid>
          <Paper>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Code</TableCell>
                  <TableCell>Requires Approval</TableCell>
                  <TableCell>Requires Attachment</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {leaveTypes.length === 0 ? (
                  <TableRow><TableCell colSpan={6} align="center"><Alert severity="info">No leave types found</Alert></TableCell></TableRow>
                ) : (
                  leaveTypes.map((lt) => (
                    <TableRow key={lt.id} hover>
                      <TableCell>{lt.name}</TableCell>
                      <TableCell><Chip label={lt.code} size="small" /></TableCell>
                      <TableCell>{lt.requiresApproval ? 'Yes' : 'No'}</TableCell>
                      <TableCell>{lt.requiresAttachment ? 'Yes' : 'No'}</TableCell>
                      <TableCell>
                        <Chip label={lt.isActive ? 'Active' : 'Inactive'} color={lt.isActive ? 'success' : 'default'} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={() => handleToggleStatus(lt.id, lt.isActive)}>
                          {lt.isActive ? <ToggleOnIcon color="success" /> : <ToggleOffIcon />}
                        </IconButton>
                        <IconButton size="small" onClick={() => openDialog('leave_type', lt)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </Paper>
        </Box>
      )}

      {activeTab === 1 && (
        <Box>
          <Grid container justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
            <Typography variant="h6">Calendar & Configuration</Typography>
            <Box display="flex" gap={1}>
              <Button variant="outlined" onClick={handleInitializeBalances} disabled={!activeCalendar}>Initialize Leave Balances</Button>
              <Button variant="contained" startIcon={<AddIcon />} onClick={() => openDialog('calendar')}>Add Calendar</Button>
            </Box>
          </Grid>

          <Paper sx={{ p: 2, mb: 3 }}>
            <Typography variant="subtitle1" gutterBottom fontWeight={600}>Active Calendar</Typography>
            {activeCalendar ? (
              <Grid container spacing={2}>
                <Grid item xs={3}><Typography variant="body2" color="text.secondary">Name</Typography><Typography>{activeCalendar.name}</Typography></Grid>
                <Grid item xs={3}><Typography variant="body2" color="text.secondary">Year</Typography><Typography>{activeCalendar.year}</Typography></Grid>
                <Grid item xs={3}><Typography variant="body2" color="text.secondary">Start Date</Typography><Typography>{activeCalendar.startDate?.substring(0, 10)}</Typography></Grid>
                <Grid item xs={3}><Typography variant="body2" color="text.secondary">End Date</Typography><Typography>{activeCalendar.endDate?.substring(0, 10)}</Typography></Grid>
              </Grid>
            ) : (
              <Alert severity="warning">No active calendar found. Create one to get started.</Alert>
            )}
          </Paper>

          <Grid container justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
            <Typography variant="h6">Leave Type Configurations</Typography>
            <Button variant="contained" startIcon={<AddIcon />} onClick={() => openDialog('config')} disabled={!activeCalendar}>Add Configuration</Button>
          </Grid>

          <Paper>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Leave Type</TableCell>
                  <TableCell>Total Days</TableCell>
                  <TableCell>Paid/Unpaid</TableCell>
                  <TableCell>Gender</TableCell>
                  <TableCell>User Type</TableCell>
                  <TableCell>Min Days</TableCell>
                  <TableCell>Max Days</TableCell>
                  <TableCell>Active</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {leaveConfigs.length === 0 ? (
                  <TableRow><TableCell colSpan={9} align="center"><Alert severity="info">No configurations found for this calendar</Alert></TableCell></TableRow>
                ) : (
                  leaveConfigs.map((cfg) => (
                    <TableRow key={cfg.id} hover>
                      <TableCell>{cfg.leaveTypeName || cfg.leaveTypeCode || cfg.leaveTypeId}</TableCell>
                      <TableCell>{cfg.totalDays}</TableCell>
                      <TableCell><Chip label={cfg.isPaid ? 'Paid' : 'Unpaid'} size="small" color={cfg.isPaid ? 'success' : 'warning'} /></TableCell>
                      <TableCell><Chip label={genderLabel(cfg.applicableGender)} size="small" /></TableCell>
                      <TableCell><Chip label={cfg.applicableUserType} size="small" /></TableCell>
                      <TableCell>{cfg.minimumDays}</TableCell>
                      <TableCell>{cfg.maximumDays}</TableCell>
                      <TableCell>
                        <Chip label={cfg.isActive ? 'Active' : 'Inactive'} color={cfg.isActive ? 'success' : 'default'} size="small" />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={() => handleToggleConfigActive(cfg.id, cfg.isActive)}>
                          {cfg.isActive ? <ToggleOnIcon color="success" /> : <ToggleOffIcon />}
                        </IconButton>
                        <IconButton size="small" onClick={() => openDialog('config', cfg)}>
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </Paper>
        </Box>
      )}

      <Dialog open={dialogOpen} onClose={closeDialog} maxWidth="sm" fullWidth>
        <DialogTitle>
          {editingItem ? 'Edit' : 'Create'}{' '}
          {dialogType === 'leave_type' && 'Leave Type'}
          {dialogType === 'calendar' && 'Calendar'}
          {dialogType === 'config' && 'Leave Configuration'}
        </DialogTitle>
        <DialogContent>
          {dialogType === 'leave_type' && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12}>
                <TextField fullWidth label="Name" value={leaveTypeForm.name} onChange={(e) => setLeaveTypeForm({ ...leaveTypeForm, name: e.target.value })} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Code" value={leaveTypeForm.code} onChange={(e) => setLeaveTypeForm({ ...leaveTypeForm, code: e.target.value })} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Description" multiline rows={3} value={leaveTypeForm.description} onChange={(e) => setLeaveTypeForm({ ...leaveTypeForm, description: e.target.value })} />
              </Grid>
              <Grid item xs={6}>
                <FormControlLabel control={<Switch checked={leaveTypeForm.requiresApproval} onChange={(e) => setLeaveTypeForm({ ...leaveTypeForm, requiresApproval: e.target.checked })} />} label="Requires Approval" />
              </Grid>
              <Grid item xs={6}>
                <FormControlLabel control={<Switch checked={leaveTypeForm.requiresAttachment} onChange={(e) => setLeaveTypeForm({ ...leaveTypeForm, requiresAttachment: e.target.checked })} />} label="Requires Attachment" />
              </Grid>
            </Grid>
          )}

          {dialogType === 'calendar' && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12}>
                <TextField fullWidth label="Name" value={calendarForm.name} onChange={(e) => setCalendarForm({ ...calendarForm, name: e.target.value })} />
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Year" type="number" value={calendarForm.year} onChange={(e) => setCalendarForm({ ...calendarForm, year: parseInt(e.target.value) || new Date().getFullYear() })} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Start Date" type="date" InputLabelProps={{ shrink: true }} value={calendarForm.startDate} onChange={(e) => setCalendarForm({ ...calendarForm, startDate: e.target.value })} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="End Date" type="date" InputLabelProps={{ shrink: true }} value={calendarForm.endDate} onChange={(e) => setCalendarForm({ ...calendarForm, endDate: e.target.value })} />
              </Grid>
            </Grid>
          )}

          {dialogType === 'config' && (
            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12}>
                <FormControl fullWidth>
                  <InputLabel>Leave Type</InputLabel>
                  <Select value={configForm.leaveTypeId} label="Leave Type" onChange={(e) => setConfigForm({ ...configForm, leaveTypeId: e.target.value })}>
                    {leaveTypes.map((lt) => (<MenuItem key={lt.id} value={lt.id}>{lt.name} ({lt.code})</MenuItem>))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12}>
                <TextField fullWidth label="Total Days" type="number" value={configForm.totalDays} onChange={(e) => setConfigForm({ ...configForm, totalDays: e.target.value })} />
              </Grid>
              <Grid item xs={6}>
                <FormControlLabel control={<Switch checked={configForm.isPaid} onChange={(e) => setConfigForm({ ...configForm, isPaid: e.target.checked })} />} label="Paid" />
              </Grid>
              <Grid item xs={6}>
                <FormControlLabel control={<Switch checked={configForm.isActive} onChange={(e) => setConfigForm({ ...configForm, isActive: e.target.checked })} />} label="Active" />
              </Grid>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>Gender</InputLabel>
                  <Select value={configForm.applicableGender} label="Gender" onChange={(e) => setConfigForm({ ...configForm, applicableGender: e.target.value })}>
                    <MenuItem value={1}>Male</MenuItem>
                    <MenuItem value={2}>Female</MenuItem>
                    <MenuItem value={3}>Other</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6}>
                <FormControl fullWidth>
                  <InputLabel>User Type</InputLabel>
                  <Select value={configForm.applicableUserType} label="User Type" onChange={(e) => setConfigForm({ ...configForm, applicableUserType: e.target.value })}>
                    <MenuItem value="Both">Both</MenuItem>
                    <MenuItem value="Teacher">Teacher</MenuItem>
                    <MenuItem value="Employee">Employee</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Min Days per Request" type="number" value={configForm.minimumDays} onChange={(e) => setConfigForm({ ...configForm, minimumDays: e.target.value })} />
              </Grid>
              <Grid item xs={6}>
                <TextField fullWidth label="Max Days per Request" type="number" value={configForm.maximumDays} onChange={(e) => setConfigForm({ ...configForm, maximumDays: e.target.value })} />
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={closeDialog}>Cancel</Button>
          <Button variant="contained" onClick={handleSubmit} disabled={submitting}>
            {submitting ? <CircularProgress size={24} /> : editingItem ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
