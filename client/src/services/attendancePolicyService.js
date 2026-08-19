import axiosInstance from './axiosInstance';

const attendancePolicyService = {
  getPolicy: (schoolId) => axiosInstance.get(`/attendance-policy/${schoolId}`),
  updatePolicy: (schoolId, data) => axiosInstance.put(`/attendance-policy/${schoolId}`, data),
  getMonthlySummaries: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.month) queryParams.append('month', params.month);
    if (params.year) queryParams.append('year', params.year);
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    return axiosInstance.get(`/attendance-policy/monthly-summary?${queryParams.toString()}`);
  },
};

export default attendancePolicyService;
