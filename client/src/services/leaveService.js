import axiosInstance from './axiosInstance';

const leaveService = {
  getLeaveTypesForUser: () => axiosInstance.get('/leaves/types'),
  getMyBalance: () => axiosInstance.get('/leaves/balance'),
  applyLeave: (data) => axiosInstance.post('/leaves', data),
  getMyRequests: () => axiosInstance.get('/leaves/my-requests'),
  cancelLeave: (id) => axiosInstance.put(`/leaves/${id}/cancel`),

  // Admin
  getCalendars: () => axiosInstance.get('/admin/leaves/calendar'),
  createCalendar: (data) => axiosInstance.post('/admin/leaves/calendar', data),
  updateCalendar: (id, data) => axiosInstance.put(`/admin/leaves/calendar/${id}`, data),
  getActiveCalendar: () => axiosInstance.get('/admin/leaves/calendar/active'),
  getLeaveTypes: () => axiosInstance.get('/admin/leaves/types'),
  createLeaveType: (data) => axiosInstance.post('/admin/leaves/types', data),
  updateLeaveType: (id, data) => axiosInstance.put(`/admin/leaves/types/${id}`, data),
  getLeaveConfigs: (calendarId) => axiosInstance.get(`/admin/leaves/calendar/${calendarId}/configs`),
  getCalendarConfigs: (calendarId) => axiosInstance.get(`/admin/leaves/calendar/${calendarId}/configs`),
  createLeaveConfig: (calendarId, data) => axiosInstance.post(`/admin/leaves/calendar/${calendarId}/configs`, data),
  createConfig: (calendarId, data) => axiosInstance.post(`/admin/leaves/calendar/${calendarId}/configs`, data),
  updateLeaveConfig: (id, data) => axiosInstance.put(`/admin/leaves/configs/${id}`, data),
  updateConfig: (id, data) => axiosInstance.put(`/admin/leaves/configs/${id}`, data),
  initializeBalances: (calendarId) => axiosInstance.post(`/admin/leaves/calendar/${calendarId}/initialize-balances`),
  getAllRequests: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    return axiosInstance.get(`/admin/leaves?${queryParams.toString()}`);
  },
  getPendingRequests: () => axiosInstance.get('/admin/leaves/pending'),
  approveLeave: (id, data = {}) => axiosInstance.put(`/admin/leaves/${id}/approve`, data),
  rejectLeave: (id, data) => axiosInstance.put(`/admin/leaves/${id}/reject`, data),
  getUserBalances: (userId) => axiosInstance.get(`/admin/leaves/balances/${userId}`),
};

export default leaveService;
