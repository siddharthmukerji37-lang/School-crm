import axiosInstance from './axiosInstance';

const examService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.classRoomId) queryParams.append('classRoomId', params.classRoomId);
    if (params.sortColumn) queryParams.append('sortColumn', params.sortColumn);
    if (params.sortOrder) queryParams.append('sortOrder', params.sortOrder);
    return axiosInstance.get(`/exams?${queryParams.toString()}`);
  },
  getById: (id) => axiosInstance.get(`/exams/${id}`),
  create: (data) => axiosInstance.post('/exams', data),
  update: (id, data) => axiosInstance.put(`/exams/${id}`, data),
  delete: (id) => axiosInstance.delete(`/exams/${id}`),
  getSchedules: (id) => axiosInstance.get(`/exams/${id}/schedules`),
  createSchedule: (id, data) => axiosInstance.post(`/exams/${id}/schedules`, data),
  enterMarks: (id, data) => axiosInstance.post(`/exams/${id}/marks`, data),
  getResults: (id, params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    return axiosInstance.get(`/exams/${id}/results?${queryParams.toString()}`);
  },
  publishResults: (id) => axiosInstance.post(`/exams/${id}/publish`),
};

export default examService;
