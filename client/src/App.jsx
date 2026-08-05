import React, { Suspense } from 'react';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';
import AppRouter from './routes/AppRouter';

const LoadingFallback = () => (
  <Box
    display="flex"
    justifyContent="center"
    alignItems="center"
    minHeight="100vh"
  >
    <CircularProgress size={48} />
  </Box>
);

export default function App() {
  return (
    <Suspense fallback={<LoadingFallback />}>
      <AppRouter />
    </Suspense>
  );
}
