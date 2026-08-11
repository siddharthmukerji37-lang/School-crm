import axiosInstance from '../services/axiosInstance';

export async function uploadFile(file) {
  const formData = new FormData();
  formData.append('file', file);

  const response = await axiosInstance.post('/files/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
    timeout: 60000,
  });

  return response.data;
}

export function fileUrl(path) {
  if (!path) return null;
  if (path.startsWith('http')) return path;
  return path;
}
