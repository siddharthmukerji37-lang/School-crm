import axiosInstance from './axiosInstance';

const payrollService = {
  getMySalaryProfile: () => axiosInstance.get('/payroll/my-profile'),
  getMyPayrolls: () => axiosInstance.get('/payroll/my-payrolls'),
  getMyPayslip: (payrollId) => axiosInstance.get(`/payroll/my-payrolls/${payrollId}/payslip`),
  getMyPayslips: () => axiosInstance.get('/payroll/my-payslips'),

  getSettings: () => axiosInstance.get('/admin/payroll/settings'),
  saveSettings: (data) => axiosInstance.post('/admin/payroll/settings', data),
  getAllSalaryProfiles: () => axiosInstance.get('/admin/payroll/salary-profiles'),
  getSalaryProfile: (userId) => axiosInstance.get(`/admin/payroll/salary-profiles/${userId}`),
  createSalaryProfile: (data) => axiosInstance.post('/admin/payroll/salary-profiles', data),
  updateSalaryProfile: (id, data) => axiosInstance.put(`/admin/payroll/salary-profiles/${id}`, data),
  getComponents: (profileId) => axiosInstance.get(`/admin/payroll/salary-profiles/${profileId}/components`),
  addComponent: (profileId, data) => axiosInstance.post(`/admin/payroll/salary-profiles/${profileId}/components`, data),
  updateComponent: (id, data) => axiosInstance.put(`/admin/payroll/components/${id}`, data),
  deleteComponent: (id) => axiosInstance.delete(`/admin/payroll/components/${id}`),
  generatePayroll: (data) => axiosInstance.post('/admin/payroll/generate', data),
  getPayrolls: (month, year) => axiosInstance.get(`/admin/payroll/payrolls?month=${month}&year=${year}`),
  getPayroll: (id) => axiosInstance.get(`/admin/payroll/payrolls/${id}`),
  approvePayroll: (id) => axiosInstance.post(`/admin/payroll/payrolls/${id}/approve`),
  markPaid: (id) => axiosInstance.post(`/admin/payroll/payrolls/${id}/mark-paid`),
  generatePayslip: (id) => axiosInstance.post(`/admin/payroll/payrolls/${id}/generate-payslip`),
  getReport: (month, year) => axiosInstance.get(`/admin/payroll/report?month=${month}&year=${year}`),
};

export default payrollService;
