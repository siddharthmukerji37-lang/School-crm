import React, { Suspense, useEffect } from 'react';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';
import { useDispatch, useSelector } from 'react-redux';
import AppRouter from './routes/AppRouter';
import { fetchCurrentUser } from './store/slices/authSlice';

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
  const dispatch = useDispatch();
  const { token, user } = useSelector((state) => state.auth);

  useEffect(() => {
    if (token && !user) {
      dispatch(fetchCurrentUser());
    }
  }, [dispatch, token, user]);

  return (
    <Suspense fallback={<LoadingFallback />}>
      <AppRouter />
    </Suspense>
  );
}
