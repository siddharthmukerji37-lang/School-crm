import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useDispatch, useSelector } from 'react-redux';
import { Box, Paper, Typography, Avatar, Chip, Button, Divider, CircularProgress, Table, TableBody, TableCell, TableContainer, TableHead, TableRow } from '@mui/material';
import Grid from '@mui/material/Grid2';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { fetchParentById, clearSelectedParent } from '../../store/slices/parentSlice';

function DetailRow({ label, value }) {
  return (
    <Box sx={{ display: 'flex', py: 1 }}>
      <Typography variant="body2" color="text.secondary" sx={{ minWidth: 180, fontWeight: 500 }}>
        {label}
      </Typography>
      <Typography variant="body2">{value || '-'}</Typography>
    </Box>
  );
}

export default function ParentDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const dispatch = useDispatch();
  const { selectedParent, loading } = useSelector((state) => state.parents);

  useEffect(() => {
    dispatch(fetchParentById(id));
    return () => {
      dispatch(clearSelectedParent());
    };
  }, [dispatch, id]);

  if (loading || !selectedParent) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  const parent = selectedParent;

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/parents')}
          variant="outlined"
        >
          Back
        </Button>
        <Box sx={{ flex: 1 }}>
          <Typography variant="h4" fontWeight={700}>
            Parent Profile
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<EditIcon />}
          onClick={() => navigate(`/parents/${id}/edit`)}
        >
          Edit
        </Button>
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, flexWrap: 'wrap' }}>
          <Avatar
            sx={{
              width: 80,
              height: 80,
              bgcolor: 'primary.main',
              fontSize: '1.75rem',
              fontWeight: 700,
            }}
          >
            {parent.firstName?.charAt(0)}
            {parent.lastName?.charAt(0)}
          </Avatar>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h5" fontWeight={600}>
              {parent.firstName} {parent.lastName}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, mt: 1, flexWrap: 'wrap' }}>
              <Chip label={parent.relationship || 'N/A'} color="primary" size="small" />
              <Chip
                label={parent.isActive ? 'Active' : 'Inactive'}
                color={parent.isActive ? 'success' : 'default'}
                size="small"
                variant="outlined"
              />
            </Box>
          </Box>
        </Box>
      </Paper>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
          Contact Information
        </Typography>
        <Divider sx={{ mb: 1 }} />
        <Grid container spacing={3}>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="First Name" value={parent.firstName} />
            <DetailRow label="Last Name" value={parent.lastName} />
            <DetailRow label="Email" value={parent.email} />
            <DetailRow label="Phone" value={parent.phone} />
            <DetailRow label="Alternative Phone" value={parent.alternativePhone} />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <DetailRow label="Occupation" value={parent.occupation} />
            <DetailRow label="Relationship" value={parent.relationship} />
            <DetailRow label="Address" value={parent.address} />
            <DetailRow label="City" value={parent.city} />
            <DetailRow label="State" value={parent.state} />
            <DetailRow label="Country" value={parent.country} />
            <DetailRow label="Postal Code" value={parent.postalCode} />
          </Grid>
        </Grid>
      </Paper>

      <Paper sx={{ p: 3 }}>
        <Typography variant="subtitle1" fontWeight={600} gutterBottom sx={{ color: 'primary.main' }}>
          Children ({parent.children?.length || 0})
        </Typography>
        <Divider sx={{ mb: 2 }} />
        {parent.children?.length ? (
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Student Name</TableCell>
                  <TableCell>Admission Number</TableCell>
                  <TableCell>Class</TableCell>
                  <TableCell>Section</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {parent.children.map((child) => (
                  <TableRow key={child.studentId}>
                    <TableCell>{child.studentName}</TableCell>
                    <TableCell>{child.admissionNumber || '-'}</TableCell>
                    <TableCell>{child.className || '-'}</TableCell>
                    <TableCell>{child.sectionName || '-'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        ) : (
          <Typography variant="body2" color="text.secondary">
            No children linked to this parent.
          </Typography>
        )}
      </Paper>
    </Box>
  );
}
