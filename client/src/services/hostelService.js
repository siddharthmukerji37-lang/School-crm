import axiosInstance from './axiosInstance';

const hostelService = {
  getAll: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    return axiosInstance.get(`/hostel?${queryParams.toString()}`);
  },
  getById: (id) => axiosInstance.get(`/hostel/${id}`),
  create: (data) => axiosInstance.post('/hostel', data),
  update: (id, data) => axiosInstance.put(`/hostel/${id}`, data),
  delete: (id) => axiosInstance.delete(`/hostel/${id}`),
  getRooms: (hostelId) => axiosInstance.get(`/hostel/${hostelId}/rooms`),
  getAllRooms: () => axiosInstance.get('/hostel/rooms'),
  createRoom: (data) => axiosInstance.post('/hostel/rooms', data),
  updateRoom: (id, data) => axiosInstance.put(`/hostel/rooms/${id}`, data),
  deleteRoom: (id) => axiosInstance.delete(`/hostel/rooms/${id}`),
  getBeds: (roomId) => axiosInstance.get(`/hostel/rooms/${roomId}/beds`),
  getAllocations: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.hostelId) queryParams.append('hostelId', params.hostelId);
    return axiosInstance.get(`/hostel/allocations?${queryParams.toString()}`);
  },
  allocate: (data) => axiosInstance.post('/hostel/allocate', data),
  checkout: (allocationId) => axiosInstance.post(`/hostel/checkout/${allocationId}`),
};

export default hostelService;
