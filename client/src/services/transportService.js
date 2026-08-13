import axiosInstance from './axiosInstance';

const transportService = {
  getRoutes: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    return axiosInstance.get(`/transport/routes?${queryParams.toString()}`);
  },
  getRouteById: (id) => axiosInstance.get(`/transport/routes/${id}`),
  createRoute: (data) => axiosInstance.post('/transport/routes', data),
  updateRoute: (id, data) => axiosInstance.put(`/transport/routes/${id}`, data),
  deleteRoute: (id) => axiosInstance.delete(`/transport/routes/${id}`),
  getVehicles: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    return axiosInstance.get(`/transport/vehicles?${queryParams.toString()}`);
  },
  getVehicleById: (id) => axiosInstance.get(`/transport/vehicles/${id}`),
  createVehicle: (data) => axiosInstance.post('/transport/vehicles', data),
  updateVehicle: (id, data) => axiosInstance.put(`/transport/vehicles/${id}`, data),
  deleteVehicle: (id) => axiosInstance.delete(`/transport/vehicles/${id}`),
  getAllocations: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.studentId) queryParams.append('studentId', params.studentId);
    return axiosInstance.get(`/transport/allocations?${queryParams.toString()}`);
  },
  allocate: (data) => axiosInstance.post('/transport/allocate', data),
  deallocate: (allocationId) => axiosInstance.delete(`/transport/deallocate/${allocationId}`),
};

export default transportService;
