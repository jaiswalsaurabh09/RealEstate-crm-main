import { useEffect, useState } from 'react';
import api from '../api/client';

type AuditLog = {
  id: string;
  employeeId?: string;
  employeeName?: string;
  projectId?: string;
  projectName?: string;
  leadId?: string;

  action?: string;
  changedByUserName?: string;
  changedAt?: string;

  oldValue?: string;
  newValue?: string;
  reason?: string;
};

type Employee = {
  id: string;
  name: string;
  email: string;
};

type Project = {
  id: string;
  name: string;
};

type Lead = {
  id: string;
  name?: string;
  mobileNumber?: string;
};

function items(payload: any): any[] {
  const value = payload?.data?.data ?? payload?.data ?? payload;

  if (Array.isArray(value)) return value;

  return value?.items ||
         value?.data ||
         value?.results ||
         [];
}

export default function AuditPage() {
  const [type, setType] = useState('employee');

  const [employees, setEmployees] = useState<Employee[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [leads, setLeads] = useState<Lead[]>([]);

  const [selectedId, setSelectedId] = useState('');
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(false);

  async function loadMasterData() {
    try {
      const [employeeResponse, projectResponse, leadResponse] =
        await Promise.all([
          api.get('/employees', {
            params: { page: 1, pageSize: 100 },
          }),
          api.get('/projects', {
            params: { includeInactive: true },
          }),
          api.get('/leads', {
            params: { page: 1, pageSize: 100 },
          }),
        ]);

      setEmployees(items(employeeResponse.data));
      setProjects(items(projectResponse.data));
      setLeads(items(leadResponse.data));
    } catch (error) {
      console.error('Failed to load audit filters', error);
    }
  }

  async function loadAudit(
    auditType: string,
    id: string
  ) {
    if (!id) {
      setLogs([]);
      return;
    }

    setLoading(true);

    try {
      let response;

      if (auditType === 'employee') {
        response = await api.get(`/employees/${id}/audit`);
      } else if (auditType === 'project') {
        response = await api.get(`/projects/${id}/audit`);
      } else {
        response = await api.get(`/leads/${id}/audit`);
      }

      setLogs(items(response.data));
    } catch (error) {
      console.error('Failed to load audit logs', error);
      setLogs([]);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadMasterData();
  }, []);

  useEffect(() => {
    setSelectedId('');
    setLogs([]);
  }, [type]);

  useEffect(() => {
    loadAudit(type, selectedId);
  }, [selectedId, type]);

  const options =
    type === 'employee'
      ? employees
      : type === 'project'
        ? projects
        : leads;

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold">
          Audit Logs
        </h1>

        <p className="text-sm text-slate-500">
          Track CRM changes
        </p>
      </div>

      <div className="mb-5 flex flex-wrap gap-3">
        <select
          value={type}
          onChange={(e) => setType(e.target.value)}
          className="rounded-lg border bg-white px-4 py-2.5 text-sm"
        >
          <option value="employee">
            Employee Audit
          </option>

          <option value="project">
            Project Audit
          </option>

          <option value="lead">
            Lead Audit
          </option>
        </select>

        <select
          value={selectedId}
          onChange={(e) => setSelectedId(e.target.value)}
          className="min-w-[260px] rounded-lg border bg-white px-4 py-2.5 text-sm"
        >
          <option value="">
            Select {type}
          </option>

          {options.map((item: any) => (
            <option key={item.id} value={item.id}>
              {item.name ||
                item.projectName ||
                item.mobileNumber ||
                item.email ||
                item.id}
            </option>
          ))}
        </select>
      </div>

      <div className="overflow-hidden rounded-xl border bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-[1000px] w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-4 py-3">Date</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Action</th>
                <th className="px-4 py-3">Old Value</th>
                <th className="px-4 py-3">New Value</th>
                <th className="px-4 py-3">Reason</th>
              </tr>
            </thead>

            <tbody className="divide-y">
              {loading && (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-8 text-center text-slate-500"
                  >
                    Loading audit logs...
                  </td>
                </tr>
              )}

              {!loading && logs.length === 0 && (
                <tr>
                  <td
                    colSpan={6}
                    className="px-4 py-8 text-center text-slate-500"
                  >
                    {selectedId
                      ? 'No audit records found.'
                      : `Select a ${type} to view audit history.`}
                  </td>
                </tr>
              )}

              {!loading &&
                logs.map((log) => (
                  <tr key={log.id}>
                    <td className="px-4 py-3 whitespace-nowrap">
                      {log.changedAt
                        ? new Date(
                            log.changedAt
                          ).toLocaleString()
                        : '-'}
                    </td>

                    <td className="px-4 py-3">
                      {log.changedByUserName || '-'}
                    </td>

                    <td className="px-4 py-3">
                      <span className="rounded bg-indigo-50 px-2 py-1 text-xs font-medium text-indigo-700">
                        {log.action || '-'}
                      </span>
                    </td>

                    <td className="px-4 py-3">
                      {log.oldValue || '-'}
                    </td>

                    <td className="px-4 py-3">
                      {log.newValue || '-'}
                    </td>

                    <td className="px-4 py-3">
                      {log.reason || '-'}
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
