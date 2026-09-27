import api from './client';
import type { CreateLead, Lead } from '../types/api';

export async function getLeads(params?: {
  page?: number;
  pageSize?: number;
  search?: string;
  response?: string;
  projectId?: string;
  assignedEmployeeId?: string;
}) {
  const response = await api.get('/leads', { params });
  return response.data;
}

export async function getLead(id: string) {
  const response = await api.get(`/leads/${id}`);
  return response.data;
}

export async function createLead(data: CreateLead) {
  const response = await api.post('/leads', data);
  return response.data;
}

export async function updateLead(id: string, data: CreateLead) {
  const response = await api.put(`/leads/${id}`, data);
  return response.data;
}

export async function deleteLead(id: string) {
  const response = await api.delete(`/leads/${id}`);
  return response.data;
}

export async function assignLead(
  id: string,
  assignedEmployeeId?: string
) {
  const response = await api.post(`/leads/${id}/assign`, {
    assignedEmployeeId: assignedEmployeeId || null,
  });

  return response.data;
}

export async function getLeadAudit(id: string) {
  const response = await api.get(`/leads/${id}/audit`);
  return response.data;
}


export async function bulkAssignUnassignedLeads(
  assignedEmployeeId: string
) {
  const response = await api.post('/leads/bulk-assign-unassigned', {
    assignedEmployeeId,
  });

  return response.data;
}
