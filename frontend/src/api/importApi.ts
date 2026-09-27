import api from './client';

export async function importLeads(
  file: File,
  portalType: string
) {
  const formData = new FormData();

  formData.append('file', file);
  formData.append('portalType', portalType);

  const response = await api.post(
    '/lead-import',
    formData,
    {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    }
  );

  return response.data;
}

export async function getImportHistory() {
  const response = await api.get('/lead-import/history');
  return response.data;
}
