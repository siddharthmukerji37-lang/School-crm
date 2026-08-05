import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useSelector } from 'react-redux';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';

export default function ProtectedRoute({ children, allowedRoles }) {
  const { isAuthenticated, user, token } = useSelector((state) => state.auth);
  const location = useLocation();

  if (!isAuthenticated || !token) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (allowedRoles && allowedRoles.length > 0 && user) {
    const userRoles = user.roles || (user.role ? [user.role] : []);
    if (!userRoles.some((r) => allowedRoles.includes(r))) {
      return <Navigate to="/dashboard" replace />;
    }
  }

  return children;
}
