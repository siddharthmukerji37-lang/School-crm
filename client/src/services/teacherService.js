import axiosInstance from './axiosInstance';

const teacherService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('search', params.search);
    if (params.department) queryParams.append('department', params.department);
    if (params.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params.sortOrder) queryParams.append('sortOrder', params.sortOrder);
    return axiosInstance.get(`/teachers?${queryParams.toString()}`);
  },

  getById: (id) =>
    axiosInstance.get(`/teachers/${id}`),

  create: (data) =>
    axiosInstance.post('/teachers', data),

  update: (id, data) =>
    axiosInstance.put(`/teachers/${id}`, data),

  delete: (id) =>
    axiosInstance.delete(`/teachers/${id}`),
};

export default teacherService;
