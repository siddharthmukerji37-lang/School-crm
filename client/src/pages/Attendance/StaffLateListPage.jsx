import React, { useEffect, useState } from 'react';
import { Box, Paper, Typography, TextField, MenuItem, CircularProgress, Alert } from '@mui/material';
import Grid from '@mui/material/Grid2';
import attendanceService from '../../services/attendanceService';

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

export default function StaffLateListPage() {
  const [lateRecords, setLateRecords] = useState([]);
  const [loading, setLoading] = useState(true);
  const now = new Date();
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [year, setYear] = useState(now.getFullYear());
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  const loadData = async () => {
    setLoading(true);
    try {
      const res = await attendanceService.getLateStaff({ month, year, page, pageSize: 20 });
      setLateRecords(res.data.data?.items || []);
      setTotalCount(res.data.data?.totalCount || 0);
    } catch {
      setLateRecords([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { loadData(); }, [month, year, page]);

  return (
    <Box>
      <Typography variant="h4" fontWeight={700} sx={{ mb: 3 }}>
        Staff Late Arrivals
      </Typography>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Grid container spacing={2} alignItems="center">
          <Grid size={{ xs: 12, md: 3 }}>
            <TextField fullWidth size="small" select label="Month" value={month}
              onChange={(e) => { setMonth(parseInt(e.target.value)); setPage(1); }}>
              {MONTHS.map((m, i) => <MenuItem key={i + 1} value={i + 1}>{m}</MenuItem>)}
            </TextField>
          </Grid>
          <Grid size={{ xs: 12, md: 3 }}>
            <TextField fullWidth size="small" label="Year" type="number" value={year}
              onChange={(e) => { setYear(parseInt(e.target.value)); setPage(1); }} />
          </Grid>
        </Grid>
      </Paper>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
      ) : lateRecords.length === 0 ? (
        <Alert severity="info">No late attendance records found.</Alert>
      ) : (
        <Paper sx={{ overflow: 'hidden' }}>
          <Box sx={{ overflowX: 'auto' }}>
            <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse' }}>
              <Box component="thead">
                <Box component="tr" sx={{ borderBottom: '1px solid', borderColor: 'divider', bgcolor: 'grey.50' }}>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Name</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Type</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Date</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Actual In</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Late By</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Late Count</Box>
                  <Box component="th" sx={{ textAlign: 'left', py: 1.5, px: 2, fontWeight: 600 }}>Reason</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Policy</Box>
                  <Box component="th" sx={{ textAlign: 'center', py: 1.5, px: 2, fontWeight: 600 }}>Deduction</Box>
                </Box>
              </Box>
              <Box component="tbody">
                {lateRecords.map((r) => (
                  <Box component="tr" key={r.id} sx={{ borderBottom: '1px solid', borderColor: 'divider', '&:hover': { bgcolor: 'grey.50' } }}>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{r.name}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>
                      <span style={{ padding: '2px 8px', borderRadius: 4, fontSize: 12, background: r.role === 'Teacher' ? '#e3f2fd' : '#f3e5f5', color: r.role === 'Teacher' ? '#1565c0' : '#7b1fa2' }}>
                        {r.role}
                      </span>
                    </Box>
                    <Box component="td" sx={{ py: 1.5, px: 2 }}>{new Date(r.date).toLocaleDateString()}</Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center' }}>
                      {r.checkInTime ? `${String(Math.floor(r.checkInTime.hours || 0)).padStart(2, '0')}:${String(r.checkInTime.minutes || 0).padStart(2, '0')}` : '-'}
                    </Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center', color: 'error.main', fontWeight: 500 }}>
                      {r.lateMinutes} min
                    </Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center', fontWeight: 600 }}>
                      <span style={{ color: r.lateCountMonth > r.allowedLateCount ? '#f44336' : '#ff9800' }}>
                        {r.lateCountMonth} / {r.allowedLateCount}
                      </span>
                    </Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, maxWidth: 180, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {r.lateReason || '-'}
                    </Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center' }}>
                      {r.latePolicyExceeded ? (
                        <span style={{ color: '#f44336', fontWeight: 600 }}>Exceeded</span>
                      ) : (
                        <span style={{ color: '#4caf50' }}>Within Limit</span>
                      )}
                    </Box>
                    <Box component="td" sx={{ py: 1.5, px: 2, textAlign: 'center' }}>
                      {r.salaryDeductionRequired ? (
                        <span style={{ color: '#f44336', fontWeight: 600 }}>Required</span>
                      ) : (
                        <span>-</span>
                      )}
                    </Box>
                  </Box>
                ))}
              </Box>
            </Box>
          </Box>
          <Box sx={{ display: 'flex', justifyContent: 'center', gap: 1, py: 2 }}>
            <button disabled={page <= 1} onClick={() => setPage(page - 1)} style={{ padding: '6px 16px' }}>Previous</button>
            <Typography sx={{ alignSelf: 'center' }}>Page {page}</Typography>
            <button disabled={lateRecords.length < 20} onClick={() => setPage(page + 1)} style={{ padding: '6px 16px' }}>Next</button>
          </Box>
        </Paper>
      )}
    </Box>
  );
}
