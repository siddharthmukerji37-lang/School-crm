import axiosInstance from './axiosInstance';

const chatService = {
  getConversations: () => axiosInstance.get('/chat/conversations'),
  getMessages: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.peerUserId) queryParams.append('peerUserId', params.peerUserId);
    if (params.sectionId) queryParams.append('sectionId', params.sectionId);
    if (params.pageNumber) queryParams.append('pageNumber', params.pageNumber);
    if (params.pageSize) queryParams.append('pageSize', params.pageSize);
    return axiosInstance.get(`/chat/messages?${queryParams.toString()}`);
  },
  sendMessage: (data) => axiosInstance.post('/chat/messages', data),
  markRead: (peerUserId) => axiosInstance.post('/chat/read', { peerUserId }),
  getUsers: (params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.role) queryParams.append('role', params.role);
    if (params.search) queryParams.append('search', params.search);
    return axiosInstance.get(`/chat/users?${queryParams.toString()}`);
  },
};

export default chatService;
