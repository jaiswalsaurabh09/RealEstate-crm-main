export interface User {
  id: string;
  name: string;
  email: string;
  mobile?: string;
  employeeCode?: string;
  role: string;
  isActive?: boolean;
  isLoginEnabled?: boolean;
}

export interface ApiResponse<T> {
  success?: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

export interface PagedResult<T> {
  items?: T[];
  data?: T[];
  totalCount?: number;
  total?: number;
  page?: number;
  pageSize?: number;
  totalPages?: number;
}

export interface Lead {
  id: string;
  leadDate: string;
  leadTime: string;
  leadName: string;
  mobileNumber: string;
  email?: string;
  enquiredOn: string;
  response: string;
  responseDetails?: string;
  projectId?: string;
  projectName?: string;
  leadSource: string;
  sourceLeadId?: string;
  sourceListingId?: string;
  assignedEmployeeId?: string;
  assignedEmployeeName?: string;
  createdByUserId: string;
  createdAt: string;
  updatedAt?: string;
  importedAt?: string;
  importBatchId?: string;
}

export interface CreateLead {
  leadName: string;
  mobileNumber: string;
  email?: string;
  enquiredOn: string;
  response: string;
  responseDetails?: string;
  projectId?: string;
  leadSource: string;
  sourceLeadId?: string;
  sourceListingId?: string;
  assignedEmployeeId?: string;
}

export interface Employee {
  id: string;
  name: string;
  email: string;
  mobile?: string;
  employeeCode?: string;
  role: string;
  isActive: boolean;
  isLoginEnabled: boolean;
  createdAt?: string;
}

export interface Project {
  id: string;
  name: string;
  location: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

export interface DashboardStats {
  totalLeads?: number;
  newLeads?: number;
  assignedLeads?: number;
  convertedLeads?: number;
  activeProjects?: number;
  activeEmployees?: number;
  [key: string]: unknown;
}

export interface AuditLog {
  id: string;
  leadId?: string;
  employeeId?: string;
  projectId?: string;
  mobileNumber?: string;
  leadName?: string;
  changedByUserName?: string;
  changedAt?: string;
  columnName?: string;
  oldValue?: string;
  newValue?: string;
  action?: string;
}

export interface ImportHistory {
  id: string;
  fileName?: string;
  importBatchId?: string;
  totalRows?: number;
  insertedRows?: number;
  updatedRows?: number;
  duplicateRows?: number;
  failedRows?: number;
  importedAt?: string;
  importedByUserName?: string;
}
