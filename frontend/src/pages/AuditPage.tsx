import { useEffect, useState } from 'react';
import api from '../api/client';
import type { AuditLog } from '../types/api';

function items(payload: any): any[] {
  const value = payload?.data?.data ?? payload?.data ?? payload;
  return Array.isArray(value)
    ? value
    : value?.items || value?.data || value?.results || [];
}

export default function AuditPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [type, setType] = useState('lead');

  async function load() {
    try {
      let response;

      if (type === 'employee') {
        response = await api.get('/employees');
      } else if (type === 'project') {
        response = await api.get('/projects');
      } else {
        const leads = await api.get('/leads', {
          params: { page: 1, pageSize: 20 },
        });

        const leadItems = items(leads.data);

        if (leadItems.length > 0) {
          response = await api.get(
            `/leads/${leadItems[0].id}/audit`
          );
        } else {
          setLogs([]);
          return;
        }
      }

      setLogs(items(response.data));
    } catch {
      setLogs([]);
    }
  }

  useEffect(() => {
    load();
  }, [type]);

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

      <div className="mb-5">
        <select
          value={type}
          onChange={(e) => setType(e.target.value)}
          className="rounded-lg border bg-white px-4 py-2.5 text-sm"
        >
          <option value="lead">Lead Audit</option>
          <option value="employee">Employee Audit</option>
          <option value="project">Project Audit</option>
        </select>
      </div>

      <div className="overflow-hidden rounded-xl border bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-[900px] w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-4 py-3">Date</th>
                <th className="px-4 py-3">User</th>
                <th className="px-4 py-3">Action</th>
                <th className="px-4 py-3">Column</th>
                <th className="px-4 py-3">Old Value</th>
                <th className="px-4 py-3">New Value</th>
              </tr>
            </thead>

            <tbody className="divide-y">
              {logs.map((log) => (
                <tr key={log.id}>
                  <td className="px-4 py-3">
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
                    <span className="rounded bg-indigo-50 px-2 py-1 text-xs text-indigo-700">
                      {log.action || '-'}
                    </span>
                  </td>

                  <td className="px-4 py-3">
                    {log.columnName || '-'}
                  </td>

                  <td className="max-w-xs truncate px-4 py-3 text-slate-500">
                    {log.oldValue || '-'}
                  </td>

                  <td className="max-w-xs truncate px-4 py-3">
                    {log.newValue || '-'}
                  </td>
                </tr>
              ))}

              {logs.length === 0 && (
                <tr>
                  <td
                    colSpan={6}
                    className="p-10 text-center text-slate-500"
                  >
                    No audit records found.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
