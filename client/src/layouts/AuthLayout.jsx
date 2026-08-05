import React from 'react';
import { Outlet } from 'react-router-dom';
import { Box, Typography, Paper } from '@mui/material';
import SchoolIcon from '@mui/icons-material/School';

export default function AuthLayout() {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #0D47A1 0%, #7B1FA2 100%)',
        p: 2,
      }}
    >
      <Box
        sx={{
          width: '100%',
          maxWidth: 440,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <Paper
          elevation={8}
          sx={{
            width: '100%',
            p: { xs: 3, sm: 4 },
            borderRadius: 3,
          }}
        >
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              mb: 3,
            }}
          >
            <Box
              sx={{
                width: 56,
                height: 56,
                borderRadius: '50%',
                backgroundColor: 'primary.main',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                mb: 2,
              }}
            >
              <SchoolIcon sx={{ fontSize: 32, color: 'white' }} />
            </Box>
            <Typography variant="h5" fontWeight={700} color="text.primary">
              School CRM
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              Complete School Management System
            </Typography>
          </Box>

          <Outlet />
        </Paper>

        <Typography
          variant="body2"
          color="rgba(255,255,255,0.7)"
          sx={{ mt: 3, textAlign: 'center' }}
        >
          &copy; {new Date().getFullYear()} School CRM. All rights reserved.
        </Typography>
      </Box>
    </Box>
  );
}
