import axiosInstance from './axiosInstance';

const authService = {
  login: (credentials) =>
    axiosInstance.post('/auth/login', credentials),

  register: (userData) =>
    axiosInstance.post('/auth/register', userData),

  refreshToken: (token) =>
    axiosInstance.post('/auth/refresh', { refreshToken: token }),

  logout: () =>
    axiosInstance.post('/auth/logout'),

  forgotPassword: (email) =>
    axiosInstance.post('/auth/forgot-password', { email }),

  resetPassword: (data) =>
    axiosInstance.post('/auth/reset-password', data),

  changePassword: (data) =>
    axiosInstance.post('/auth/change-password', data),

  getProfile: () =>
    axiosInstance.get('/auth/me'),

  getMyProfile: () =>
    axiosInstance.get('/auth/my-profile'),

  updateProfile: (data) =>
    axiosInstance.put('/auth/me', data),
};

export default authService;
