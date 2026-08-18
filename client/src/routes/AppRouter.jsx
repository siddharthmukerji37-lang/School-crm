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
const StaffAttendancePage = lazy(() => import('../pages/Attendance/StaffAttendancePage'));
const MyAttendancePage = lazy(() => import('../pages/Attendance/MyAttendancePage'));
const ExamListPage = lazy(() => import('../pages/Exams/ExamListPage'));
const ExamFormPage = lazy(() => import('../pages/Exams/ExamFormPage'));
const ExamQuestionsPage = lazy(() => import('../pages/Exams/ExamQuestionsPage'));
const ExamSubmissionsPage = lazy(() => import('../pages/Exams/ExamSubmissionsPage'));
const StudentExamTakePage = lazy(() => import('../pages/Exams/StudentExamTakePage'));
const HomeworkListPage = lazy(() => import('../pages/Homework/HomeworkListPage'));
const HomeworkFormPage = lazy(() => import('../pages/Homework/HomeworkFormPage'));
const HomeworkDetailPage = lazy(() => import('../pages/Homework/HomeworkDetailPage'));
const BookListPage = lazy(() => import('../pages/Library/BookListPage'));
const BookFormPage = lazy(() => import('../pages/Library/BookFormPage'));
const BookDetailPage = lazy(() => import('../pages/Library/BookDetailPage'));
const IssuedBooksPage = lazy(() => import('../pages/Library/IssuedBooksPage'));
const MyIssuedBooksPage = lazy(() => import('../pages/Library/MyIssuedBooksPage'));
const TransportPage = lazy(() => import('../pages/Transport/TransportPage'));
const HostelPage = lazy(() => import('../pages/Hostel/HostelPage'));
const FeeStructureListPage = lazy(() => import('../pages/Fees/FeeStructureListPage'));
const FeeCollectPage = lazy(() => import('../pages/Fees/FeeCollectPage'));
const FeeReceiptListPage = lazy(() => import('../pages/Fees/FeeReceiptListPage'));
const MyFeeReceiptsPage = lazy(() => import('../pages/Fees/MyFeeReceiptsPage'));
const InventoryListPage = lazy(() => import('../pages/Inventory/InventoryListPage'));
const InventoryFormPage = lazy(() => import('../pages/Inventory/InventoryFormPage'));
const VendorFormPage = lazy(() => import('../pages/Inventory/VendorFormPage'));
const AccountsPage = lazy(() => import('../pages/Accounts/AccountsPage'));
const NotificationListPage = lazy(() => import('../pages/Notifications/NotificationListPage'));
const ReportsPage = lazy(() => import('../pages/Reports/ReportsPage'));
const SettingsPage = lazy(() => import('../pages/Settings/SettingsPage'));
const NoticeBoardPage = lazy(() => import('../pages/NoticeBoard/NoticeBoardPage'));
const NoticeManagementPage = lazy(() => import('../pages/NoticeBoard/NoticeManagementPage'));
const TimetablePage = lazy(() => import('../pages/Timetable/TimetablePage'));
const ChatPage = lazy(() => import('../pages/Chat/ChatPage'));
const ProfilePage = lazy(() => import('../pages/Profile/ProfilePage'));

const ADMIN_ROLES = ['SuperAdmin', 'Admin'];
const STAFF_ROLES = ['SuperAdmin', 'Admin', 'Teacher', 'ClassTeacher'];
const FEE_ROLES = ['SuperAdmin', 'Admin', 'Accountant'];
const ACCOUNTS_ROLES = ['SuperAdmin', 'Admin', 'Accountant'];
const INVENTORY_ROLES = ['SuperAdmin', 'Admin', 'Accountant'];
const LIBRARY_ROLES = ['SuperAdmin', 'Admin', 'Librarian'];
const GENERAL_ROLES = [
  'SuperAdmin', 'Admin', 'SchoolAdmin', 'Principal', 'VicePrincipal',
  'Teacher', 'ClassTeacher', 'Receptionist', 'Student', 'Parent',
];
const CHAT_ROLES = [
  'SuperAdmin', 'Admin', 'SchoolAdmin', 'Principal', 'VicePrincipal',
  'Teacher', 'ClassTeacher', 'Receptionist', 'Student', 'Parent',
  'Librarian', 'Accountant',
];

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

        <Route path="students" element={
          <ProtectedRoute allowedRoles={GENERAL_ROLES}><StudentListPage /></ProtectedRoute>
        } />
        <Route path="students/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><StudentFormPage /></ProtectedRoute>
        } />
        <Route path="students/:id" element={<StudentDetailPage />} />
        <Route path="students/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><StudentFormPage /></ProtectedRoute>
        } />

        <Route path="teachers" element={
          <ProtectedRoute allowedRoles={GENERAL_ROLES}><TeacherListPage /></ProtectedRoute>
        } />
        <Route path="teachers/:id" element={<TeacherDetailPage />} />
        <Route path="teachers/create" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><TeacherFormPage /></ProtectedRoute>
        } />
        <Route path="teachers/:id/edit" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><TeacherFormPage /></ProtectedRoute>
        } />

        <Route path="parents" element={
          <ProtectedRoute allowedRoles={GENERAL_ROLES}><ParentListPage /></ProtectedRoute>
        } />
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

        <Route path="attendance" element={
          <ProtectedRoute allowedRoles={GENERAL_ROLES}><AttendanceListPage /></ProtectedRoute>
        } />
        <Route path="attendance/mark" element={
          <ProtectedRoute allowedRoles={STAFF_ROLES}><AttendanceMarkPage /></ProtectedRoute>
        } />
        <Route path="attendance/staff" element={
          <ProtectedRoute allowedRoles={ADMIN_ROLES}><StaffAttendancePage /></ProtectedRoute>
        } />
        <Route path="my-attendance" element={
          <ProtectedRoute allowedRoles={STAFF_ROLES}><MyAttendancePage /></ProtectedRoute>
        } />

        <Route path="exams" element={
          <ProtectedRoute allowedRoles={GENERAL_ROLES}><ExamListPage /></ProtectedRoute>
        } />
        <Route path="exams/create" element={
          <ProtectedRoute allowedRoles={STAFF_ROLES}><ExamFormPage /></ProtectedRoute>
        } />
        <Route path="exams/:id/edit" element={
          <ProtectedRoute allowedRoles={STAFF_ROLES}><ExamFormPage /></ProtectedRoute>
        } />
        <Route path="exams/:id/questions" element={
          <ProtectedRoute allowedRoles={STAFF_ROLES}><ExamQuestionsPage /></ProtectedRoute>
        } />
        <Route path="exams/:id/submissions" element={
          <ProtectedRoute allowedRoles={STAFF_ROLES}><ExamSubmissionsPage /></ProtectedRoute>
        } />
        <Route path="exams/:id/take" element={
          <ProtectedRoute allowedRoles={['Student']}><StudentExamTakePage /></ProtectedRoute>
        } />

        <Route path="homework" element={
          <ProtectedRoute allowedRoles={GENERAL_ROLES}><HomeworkListPage /></ProtectedRoute>
        } />
        <Route path="homework/:id" element={<HomeworkDetailPage />} />
        <Route path="homework/create" element={
          <ProtectedRoute allowedRoles={STAFF_ROLES}><HomeworkFormPage /></ProtectedRoute>
        } />
        <Route path="homework/:id/edit" element={
          <ProtectedRoute allowedRoles={STAFF_ROLES}><HomeworkFormPage /></ProtectedRoute>
        } />

        <Route path="library" element={<BookListPage />} />
        <Route path="library/issued" element={<IssuedBooksPage />} />
        <Route path="library/my-issues" element={
          <ProtectedRoute allowedRoles={['Student', 'Teacher']}><MyIssuedBooksPage /></ProtectedRoute>
        } />
        <Route path="library/:id" element={<BookDetailPage />} />
        <Route path="library/create" element={
          <ProtectedRoute allowedRoles={LIBRARY_ROLES}><BookFormPage /></ProtectedRoute>
        } />
        <Route path="library/:id/edit" element={
          <ProtectedRoute allowedRoles={LIBRARY_ROLES}><BookFormPage /></ProtectedRoute>
        } />

        <Route path="transport" element={
          <ProtectedRoute allowedRoles={GENERAL_ROLES}><TransportPage /></ProtectedRoute>
        } />
        <Route path="hostel" element={
          <ProtectedRoute allowedRoles={GENERAL_ROLES}><HostelPage /></ProtectedRoute>
        } />

        <Route path="fees" element={<FeeStructureListPage />} />
        <Route path="fees/collect" element={
          <ProtectedRoute allowedRoles={FEE_ROLES}><FeeCollectPage /></ProtectedRoute>
        } />
        <Route path="fees/receipts" element={
          <ProtectedRoute allowedRoles={FEE_ROLES}><FeeReceiptListPage /></ProtectedRoute>
        } />
        <Route path="fees/my-receipts" element={
          <ProtectedRoute allowedRoles={['Student']}><MyFeeReceiptsPage /></ProtectedRoute>
        } />

        <Route path="inventory" element={
          <ProtectedRoute allowedRoles={INVENTORY_ROLES}><InventoryListPage /></ProtectedRoute>
        } />
        <Route path="inventory/create" element={
          <ProtectedRoute allowedRoles={INVENTORY_ROLES}><InventoryFormPage /></ProtectedRoute>
        } />
        <Route path="inventory/:id/edit" element={
          <ProtectedRoute allowedRoles={INVENTORY_ROLES}><InventoryFormPage /></ProtectedRoute>
        } />
        <Route path="vendors/create" element={
          <ProtectedRoute allowedRoles={INVENTORY_ROLES}><VendorFormPage /></ProtectedRoute>
        } />
        <Route path="vendors/:id/edit" element={
          <ProtectedRoute allowedRoles={INVENTORY_ROLES}><VendorFormPage /></ProtectedRoute>
        } />

        <Route path="accounts" element={
          <ProtectedRoute allowedRoles={ACCOUNTS_ROLES}><AccountsPage /></ProtectedRoute>
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

        <Route path="timetable" element={
          <ProtectedRoute allowedRoles={GENERAL_ROLES}><TimetablePage /></ProtectedRoute>
        } />

        <Route path="chat" element={
          <ProtectedRoute allowedRoles={CHAT_ROLES}><ChatPage /></ProtectedRoute>
        } />

        <Route path="profile" element={<ProfilePage />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
