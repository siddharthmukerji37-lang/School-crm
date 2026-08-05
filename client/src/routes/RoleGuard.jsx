import React from 'react';
import { Navigate } from 'react-router-dom';
import { useSelector } from 'react-redux';

export default function RoleGuard({ children, roles, fallback = '/' }) {
  const { user, isAuthenticated } = useSelector((state) => state.auth);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (roles && roles.length > 0 && user) {
    const userRoles = user.roles || (user.role ? [user.role] : []);
    if (!userRoles.some((r) => roles.includes(r))) {
      return <Navigate to={fallback} replace />;
    }
  }

  return children;
}
