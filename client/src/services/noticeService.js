import axiosInstance from './axiosInstance';

const noticeService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    return axiosInstance.get(`/notices?${queryParams.toString()}`);
  },
  getPublished: () => axiosInstance.get('/notices/published'),
  getById: (id) => axiosInstance.get(`/notices/${id}`),
  create: (data) => axiosInstance.post('/notices', data),
  update: (id, data) => axiosInstance.put(`/notices/${id}`, data),
  delete: (id) => axiosInstance.delete(`/notices/${id}`),
};

export default noticeService;
