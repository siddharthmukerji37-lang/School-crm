import axiosInstance from './axiosInstance';

const employeeService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.departmentId) queryParams.append('departmentId', params.departmentId);
    if (params.status) queryParams.append('status', params.status);
    if (params.sortColumn) queryParams.append('sortColumn', params.sortColumn);
    if (params.sortOrder) queryParams.append('sortOrder', params.sortOrder);
    return axiosInstance.get(`/employees?${queryParams.toString()}`);
  },
  getById: (id) => axiosInstance.get(`/employees/${id}`),
  create: (data) => axiosInstance.post('/employees', data),
  update: (id, data) => axiosInstance.put(`/employees/${id}`, data),
  delete: (id) => axiosInstance.delete(`/employees/${id}`),
};

export default employeeService;
