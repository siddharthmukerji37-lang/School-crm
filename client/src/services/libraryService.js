import axiosInstance from './axiosInstance';

const libraryService = {
  getAllBooks: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.category) queryParams.append('category', params.category);
    if (params.author) queryParams.append('author', params.author);
    return axiosInstance.get(`/library?${queryParams.toString()}`);
  },
  getBookById: (id) => axiosInstance.get(`/library/${id}`),
  createBook: (data) => axiosInstance.post('/library', data),
  updateBook: (id, data) => axiosInstance.put(`/library/${id}`, data),
  deleteBook: (id) => axiosInstance.delete(`/library/${id}`),
  issueBook: (data) => axiosInstance.post('/library/issue', data),
  returnBook: (issueId) => axiosInstance.post(`/library/return/${issueId}`),
  getIssuedBooks: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.page) queryParams.append('pageNumber', params.page);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    if (params.search) queryParams.append('searchTerm', params.search);
    if (params.overdue) queryParams.append('overdue', params.overdue);
    return axiosInstance.get(`/library/issued?${queryParams.toString()}`);
  },
};

export default libraryService;
