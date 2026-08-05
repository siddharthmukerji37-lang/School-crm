import axiosInstance from './axiosInstance';

const homeworkService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.classRoomId) queryParams.append('classRoomId', params.classRoomId);
    if (params.sectionId) queryParams.append('sectionId', params.sectionId);
    if (params.subjectId) queryParams.append('subjectId', params.subjectId);
    return axiosInstance.get(`/homework?${queryParams.toString()}`);
  },
  getById: (id) => axiosInstance.get(`/homework/${id}`),
  create: (data) => axiosInstance.post('/homework', data),
  update: (id, data) => axiosInstance.put(`/homework/${id}`, data),
  delete: (id) => axiosInstance.delete(`/homework/${id}`),
  submit: (id, data) => axiosInstance.post(`/homework/${id}/submit`, data),
  review: (id, data) => axiosInstance.post(`/homework/${id}/review`, data),
};

export default homeworkService;
