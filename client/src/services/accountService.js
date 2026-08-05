import axiosInstance from './axiosInstance';

const accountService = {
  getIncome: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params.toDate) queryParams.append('toDate', params.toDate);
    return axiosInstance.get(`/accounts/income?${queryParams.toString()}`);
  },
  createIncome: (data) => axiosInstance.post('/accounts/income', data),
  updateIncome: (id, data) => axiosInstance.put(`/accounts/income/${id}`, data),
  deleteIncome: (id) => axiosInstance.delete(`/accounts/income/${id}`),
  getExpense: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params.toDate) queryParams.append('toDate', params.toDate);
    return axiosInstance.get(`/accounts/expense?${queryParams.toString()}`);
  },
  createExpense: (data) => axiosInstance.post('/accounts/expense', data),
  updateExpense: (id, data) => axiosInstance.put(`/accounts/expense/${id}`, data),
  deleteExpense: (id) => axiosInstance.delete(`/accounts/expense/${id}`),
  getLedger: (params) => {
    const queryParams = new URLSearchParams();
    if (params.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params.toDate) queryParams.append('toDate', params.toDate);
    return axiosInstance.get(`/accounts/ledger?${queryParams.toString()}`);
  },
};

export default accountService;
