import axiosInstance from './axiosInstance';

const salaryDeductionService = {
  getDeductions: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.month) queryParams.append('month', params.month);
    if (params.year) queryParams.append('year', params.year);
    if (params.status) queryParams.append('status', params.status);
    return axiosInstance.get(`/salary-deductions?${queryParams.toString()}`);
  },
  approve: (id, data = {}) => axiosInstance.post(`/salary-deductions/${id}/approve`, data),
  reject: (id, data = {}) => axiosInstance.post(`/salary-deductions/${id}/reject`, data),
  getMyDeductions: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    return axiosInstance.get(`/salary-deductions/my?${queryParams.toString()}`);
  },
};

export default salaryDeductionService;
