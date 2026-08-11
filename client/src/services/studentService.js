import axiosInstance from './axiosInstance';

const studentService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('search', params.search);
    if (params.classId) queryParams.append('classId', params.classId);
    if (params.sectionId) queryParams.append('sectionId', params.sectionId);
    if (params.sortBy) queryParams.append('sortBy', params.sortBy);
    if (params.sortOrder) queryParams.append('sortOrder', params.sortOrder);
    return axiosInstance.get(`/students?${queryParams.toString()}`);
  },

  getById: (id) =>
    axiosInstance.get(`/students/${id}`),

  create: (data) =>
    axiosInstance.post('/students', data),

  update: (id, data) =>
    axiosInstance.put(`/students/${id}`, data),

  delete: (id) =>
    axiosInstance.delete(`/students/${id}`),

  promote: (id, data) =>
    axiosInstance.post(`/students/${id}/promote`, data),

  getDocuments: (id) =>
    axiosInstance.get(`/students/${id}/documents`),
};

export default studentService;
