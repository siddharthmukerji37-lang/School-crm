import axiosInstance from './axiosInstance';

const reportService = {
  getTemplates: () => axiosInstance.get('/reports/templates'),
  generateReport: (type, params = {}) => {
    const queryParams = new URLSearchParams();
    if (params.format) queryParams.append('format', params.format);
    if (params.classRoomId) queryParams.append('classRoomId', params.classRoomId);
    if (params.sectionId) queryParams.append('sectionId', params.sectionId);
    if (params.examId) queryParams.append('examId', params.examId);
    if (params.studentId) queryParams.append('studentId', params.studentId);
    if (params.fromDate) queryParams.append('fromDate', params.fromDate);
    if (params.toDate) queryParams.append('toDate', params.toDate);
    return axiosInstance.get(`/reports/${type}?${queryParams.toString()}`, {
      responseType: 'blob',
    });
  },
};

export default reportService;
