import axiosInstance from './axiosInstance';

const dashboardService = {
  getStats: () =>
    axiosInstance.get('/dashboard/stats'),

  getAttendanceChart: (params) =>
    axiosInstance.get('/dashboard/attendance-chart', { params }),

  getFeeChart: (params) =>
    axiosInstance.get('/dashboard/fee-chart', { params }),
};

export default dashboardService;
