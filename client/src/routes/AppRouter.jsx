import React, { lazy } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
import AuthLayout from '../layouts/AuthLayout';
import AdminLayout from '../layouts/AdminLayout';

const LoginPage = lazy(() => import('../pages/Auth/LoginPage'));
const ForgotPasswordPage = lazy(() => import('../pages/Auth/ForgotPasswordPage'));
const ResetPasswordPage = lazy(() => import('../pages/Auth/ResetPasswordPage'));
const DashboardPage = lazy(() => import('../pages/Dashboard/DashboardPage'));
const StudentListPage = lazy(() => import('../pages/Students/StudentListPage'));
const StudentFormPage = lazy(() => import('../pages/Students/StudentFormPage'));
const StudentDetailPage = lazy(() => import('../pages/Students/StudentDetailPage'));
const TeacherListPage = lazy(() => import('../pages/Teachers/TeacherListPage'));
const TeacherFormPage = lazy(() => import('../pages/Teachers/TeacherFormPage'));
const TeacherDetailPage = lazy(() => import('../pages/Teachers/TeacherDetailPage'));
const ParentListPage = lazy(() => import('../pages/Parents/ParentListPage'));
const ParentFormPage = lazy(() => import('../pages/Parents/ParentFormPage'));
const ParentDetailPage = lazy(() => import('../pages/Parents/ParentDetailPage'));
const EmployeeListPage = lazy(() => import('../pages/Employees/EmployeeListPage'));
const EmployeeFormPage = lazy(() => import('../pages/Employees/EmployeeFormPage'));
const EmployeeDetailPage = lazy(() => import('../pages/Employees/EmployeeDetailPage'));
const AttendanceListPage = lazy(() => import('../pages/Attendance/AttendanceListPage'));
const AttendanceMarkPage = lazy(() => import('../pages/Attendance/AttendanceMarkPage'));
const ExamListPage = lazy(() => import('../pages/Exams/ExamListPage'));
const ExamFormPage = lazy(() => import('../pages/Exams/ExamFormPage'));
const HomeworkListPage = lazy(() => import('../pages/Homework/HomeworkListPage'));
const HomeworkFormPage = lazy(() => import('../pages/Homework/HomeworkFormPage'));
const HomeworkDetailPage = lazy(() => import('../pages/Homework/HomeworkDetailPage'));
const BookListPage = lazy(() => import('../pages/Library/BookListPage'));
const BookFormPage = lazy(() => import('../pages/Library/BookFormPage'));
const BookDetailPage = lazy(() => import('../pages/Library/BookDetailPage'));
const IssuedBooksPage = lazy(() => import('../pages/Library/IssuedBooksPage'));
const TransportPage = lazy(() => import('../pages/Transport/TransportPage'));
const HostelPage = lazy(() => import('../pages/Hostel/HostelPage'));
const FeeStructureListPage = lazy(() => import('../pages/Fees/FeeStructureListPage'));
const FeeCollectPage = lazy(() => import('../pages/Fees/FeeCollectPage'));
const FeeReceiptListPage = lazy(() => import('../pages/Fees/FeeReceiptListPage'));
const InventoryListPage = lazy(() => import('../pages/Inventory/InventoryListPage'));
const InventoryFormPage = lazy(() => import('../pages/Inventory/InventoryFormPage'));
const AccountsPage = lazy(() => import('../pages/Accounts/AccountsPage'));
const NotificationListPage = lazy(() => import('../pages/Notifications/NotificationListPage'));
const ReportsPage = lazy(() => import('../pages/Reports/ReportsPage'));
const SettingsPage = lazy(() => import('../pages/Settings/SettingsPage'));
const NoticeBoardPage = lazy(() => import('../pages/NoticeBoard/NoticeBoardPage'));
const NoticeManagementPage = lazy(() => import('../pages/NoticeBoard/NoticeManagementPage'));

const ADMIN_ROLES = ['SuperAdmin', 'Admin'];

export default function AppRouter() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
      </Route>

      <Route
        path="/"
        element={
          <ProtectedRoute>
            <AdminLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />

        <Route path="students" element={<StudentListPage />} />
        <Route path="students/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><StudentFormPage /></ProtectedRoute>
        } />
        <Route path="students/:id" element={<StudentDetailPage />} />
        <Route path="students/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><StudentFormPage /></ProtectedRoute>
        } />

        <Route path="teachers" element={<TeacherListPage />} />
        <Route path="teachers/:id" element={<TeacherDetailPage />} />
        <Route path="teachers/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><TeacherFormPage /></ProtectedRoute>
        } />
        <Route path="teachers/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><TeacherFormPage /></ProtectedRoute>
        } />

        <Route path="parents" element={<ParentListPage />} />
        <Route path="parents/:id" element={<ParentDetailPage />} />
        <Route path="parents/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><ParentFormPage /></ProtectedRoute>
        } />
        <Route path="parents/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><ParentFormPage /></ProtectedRoute>
        } />

        <Route path="employees" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><EmployeeListPage /></ProtectedRoute>
        } />
        <Route path="employees/:id" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><EmployeeDetailPage /></ProtectedRoute>
        } />
        <Route path="employees/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><EmployeeFormPage /></ProtectedRoute>
        } />
        <Route path="employees/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><EmployeeFormPage /></ProtectedRoute>
        } />

        <Route path="attendance" element={<AttendanceListPage />} />
        <Route path="attendance/mark" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><AttendanceMarkPage /></ProtectedRoute>
        } />

        <Route path="exams" element={<ExamListPage />} />
        <Route path="exams/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><ExamFormPage /></ProtectedRoute>
        } />
        <Route path="exams/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><ExamFormPage /></ProtectedRoute>
        } />

        <Route path="homework" element={<HomeworkListPage />} />
        <Route path="homework/:id" element={<HomeworkDetailPage />} />
        <Route path="homework/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><HomeworkFormPage /></ProtectedRoute>
        } />
        <Route path="homework/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><HomeworkFormPage /></ProtectedRoute>
        } />

        <Route path="library" element={<BookListPage />} />
        <Route path="library/issued" element={<IssuedBooksPage />} />
        <Route path="library/:id" element={<BookDetailPage />} />
        <Route path="library/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><BookFormPage /></ProtectedRoute>
        } />
        <Route path="library/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><BookFormPage /></ProtectedRoute>
        } />

        <Route path="transport" element={<TransportPage />} />
        <Route path="hostel" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><HostelPage /></ProtectedRoute>
        } />

        <Route path="fees" element={<FeeStructureListPage />} />
        <Route path="fees/collect" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><FeeCollectPage /></ProtectedRoute>
        } />
        <Route path="fees/receipts" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><FeeReceiptListPage /></ProtectedRoute>
        } />

        <Route path="inventory" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><InventoryListPage /></ProtectedRoute>
        } />
        <Route path="inventory/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><InventoryFormPage /></ProtectedRoute>
        } />
        <Route path="inventory/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><InventoryFormPage /></ProtectedRoute>
        } />

        <Route path="accounts" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><AccountsPage /></ProtectedRoute>
        } />

        <Route path="notifications" element={<NotificationListPage />} />

        <Route path="reports" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><ReportsPage /></ProtectedRoute>
        } />

        <Route path="settings" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><SettingsPage /></ProtectedRoute>
        } />

        <Route path="notice-board" element={<NoticeBoardPage />} />
        <Route path="notices/manage" element={
          <ProtectedRoute allowedRoles={['SuperAdmin', 'Admin']}><NoticeManagementPage /></ProtectedRoute>
        } />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
