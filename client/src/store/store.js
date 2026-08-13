import { configureStore } from '@reduxjs/toolkit';
import authReducer from './slices/authSlice';
import studentReducer from './slices/studentSlice';
import teacherReducer from './slices/teacherSlice';
import parentReducer from './slices/parentSlice';
import employeeReducer from './slices/employeeSlice';
import attendanceReducer from './slices/attendanceSlice';
import examReducer from './slices/examSlice';
import homeworkReducer from './slices/homeworkSlice';
import libraryReducer from './slices/librarySlice';
import transportReducer from './slices/transportSlice';
import hostelReducer from './slices/hostelSlice';
import feeReducer from './slices/feeSlice';
import inventoryReducer from './slices/inventorySlice';
import accountReducer from './slices/accountSlice';
import dashboardReducer from './slices/dashboardSlice';
import notificationReducer from './slices/notificationSlice';
import reportReducer from './slices/reportSlice';
import uiReducer from './slices/uiSlice';
import noticeReducer from './slices/noticeSlice';
import chatReducer from './slices/chatSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    students: studentReducer,
    teachers: teacherReducer,
    parents: parentReducer,
    employees: employeeReducer,
    attendance: attendanceReducer,
    exams: examReducer,
    homework: homeworkReducer,
    library: libraryReducer,
    transport: transportReducer,
    hostel: hostelReducer,
    fees: feeReducer,
    inventory: inventoryReducer,
    accounts: accountReducer,
    dashboard: dashboardReducer,
    notifications: notificationReducer,
    reports: reportReducer,
    ui: uiReducer,
    notices: noticeReducer,
    chat: chatReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ['persist/PERSIST', 'persist/REHYDRATE'],
      },
    }),
});

export default store;
