import axiosInstance from './axiosInstance';

const attendanceService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.date) queryParams.append('date', params.date);
    if (params.classRoomId) queryParams.append('classRoomId', params.classRoomId);
    if (params.sectionId) queryParams.append('sectionId', params.sectionId);
    if (params.status) queryParams.append('status', params.status);
    return axiosInstance.get(`/attendance?${queryParams.toString()}`);
  },
  mark: (data) => axiosInstance.post('/attendance/mark', data),
  bulkMark: (data) => axiosInstance.post('/attendance/bulk-mark', data),
  getStats: (params) => {
    const queryParams = new URLSearchParams();
    if (params.date) queryParams.append('date', params.date);
    if (params.classRoomId) queryParams.append('classRoomId', params.classRoomId);
    if (params.sectionId) queryParams.append('sectionId', params.sectionId);
    return axiosInstance.get(`/attendance/stats?${queryParams.toString()}`);
  },
  getStudentAttendance: (studentId, params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    return axiosInstance.get(`/attendance/student/${studentId}?${queryParams.toString()}`);
  },
  getMy: () => axiosInstance.get('/attendance/me'),
  clockIn: () => axiosInstance.post('/attendance/me/clock-in', {}),
  clockOut: () => axiosInstance.post('/attendance/me/clock-out', {}),
  clockInWithReason: (data) => axiosInstance.post('/attendance/me/clock-in', data),
  clockOutWithReason: (data) => axiosInstance.post('/attendance/me/clock-out', data),
  getLateStaff: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.month) queryParams.append('month', params.month);
    if (params.year) queryParams.append('year', params.year);
    return axiosInstance.get(`/attendance/late-staff?${queryParams.toString()}`);
  },
  getStaff: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.date) queryParams.append('date', params.date);
    if (params.role) queryParams.append('role', params.role);
    if (params.status) queryParams.append('status', params.status);
    return axiosInstance.get(`/attendance/staff?${queryParams.toString()}`);
  },
};

export default attendanceService;
