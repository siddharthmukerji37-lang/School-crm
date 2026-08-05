import axiosInstance from './axiosInstance';

const feeService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.classRoomId) queryParams.append('classRoomId', params.classRoomId);
    return axiosInstance.get(`/fees?${queryParams.toString()}`);
  },
  getById: (id) => axiosInstance.get(`/fees/${id}`),
  create: (data) => axiosInstance.post('/fees', data),
  update: (id, data) => axiosInstance.put(`/fees/${id}`, data),
  delete: (id) => axiosInstance.delete(`/fees/${id}`),
  collect: (data) => axiosInstance.post('/fees/collect', data),
  getReceipts: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.studentId) queryParams.append('studentId', params.studentId);
    return axiosInstance.get(`/fees/receipts?${queryParams.toString()}`);
  },
  getPending: (studentId) => axiosInstance.get(`/fees/pending?studentId=${studentId}`),
  getSummary: (studentId) => axiosInstance.get(`/fees/summary/${studentId}`),
};

export default feeService;
