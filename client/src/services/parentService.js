import axiosInstance from './axiosInstance';

const parentService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.sortColumn) queryParams.append('sortColumn', params.sortColumn);
    if (params.sortOrder) queryParams.append('sortOrder', params.sortOrder);
    return axiosInstance.get(`/parents?${queryParams.toString()}`);
  },
  getById: (id) => axiosInstance.get(`/parents/${id}`),
  create: (data) => axiosInstance.post('/parents', data),
  update: (id, data) => axiosInstance.put(`/parents/${id}`, data),
  delete: (id) => axiosInstance.delete(`/parents/${id}`),
};

export default parentService;
