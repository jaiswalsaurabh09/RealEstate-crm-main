import api from './client';

export async function importLeads(file: File) {
  const formData = new FormData();
  formData.append('file', file);

  const response = await api.post('/leads/import', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  });

  return response.data;
}

export async function getImportHistory() {
  const response = await api.get('/leads/import-history');
  return response.data;
}
