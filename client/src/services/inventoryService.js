import axiosInstance from './axiosInstance';

const inventoryService = {
  getAllItems: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.category) queryParams.append('category', params.category);
    return axiosInstance.get(`/inventory?${queryParams.toString()}`);
  },
  getItemById: (id) => axiosInstance.get(`/inventory/${id}`),
  createItem: (data) => axiosInstance.post('/inventory', data),
  updateItem: (id, data) => axiosInstance.put(`/inventory/${id}`, data),
  deleteItem: (id) => axiosInstance.delete(`/inventory/${id}`),
  adjustStock: (data) => axiosInstance.post('/inventory/adjust', data),
  getVendors: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    return axiosInstance.get(`/inventory/vendors?${queryParams.toString()}`);
  },
  createVendor: (data) => axiosInstance.post('/inventory/vendors', data),
  updateVendor: (id, data) => axiosInstance.put(`/inventory/vendors/${id}`, data),
  deleteVendor: (id) => axiosInstance.delete(`/inventory/vendors/${id}`),
};

export default inventoryService;
