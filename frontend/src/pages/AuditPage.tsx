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
  leadName?: string;
  mobileNumber?: string;
};

function items(payload: any): any[] {
  const value = payload?.data?.data ?? payload?.data ?? payload;

  if (Array.isArray(value)) return value;

  return (
    value?.items ||
    value?.data ||
    value?.results ||
    []
  );
}

export default function AuditPage() {
  const [type, setType] = useState('employee');

  const [employees, setEmployees] = useState<Employee[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [leads, setLeads] = useState<Lead[]>([]);

  const [selectedId, setSelectedId] = useState('');
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(false);
  const [optionsLoading, setOptionsLoading] = useState(false);

  /*
   * Load only the list required for the selected audit type.
   * Do NOT use Promise.all() here because one failed API
   * must not prevent the other lists from loading.
   */
  useEffect(() => {
    async function loadOptions() {
      setOptionsLoading(true);
      setSelectedId('');
      setLogs([]);

      try {
        if (type === 'employee') {
          const response = await api.get('/employees', {
            params: {
              page: 1,
              pageSize: 100,
            },
          });

          setEmployees(items(response.data));
        }

        if (type === 'project') {
          const response = await api.get('/projects', {
            params: {
              includeInactive: true,
            },
          });

          const projectItems = items(response.data);

          setProjects(projectItems);
        }

        if (type === 'lead') {
          const response = await api.get('/leads', {
            params: {
              page: 1,
              pageSize: 100,
            },
          });

          setLeads(items(response.data));
        }
      } catch (error) {
        console.error(
          `Failed to load ${type} audit options`,
          error
        );
      } finally {
        setOptionsLoading(false);
      }
    }

    loadOptions();
  }, [type]);

  /*
   * Load actual audit records after selecting
   * employee/project/lead.
   */
  useEffect(() => {
    async function loadAudit() {
      if (!selectedId) {
        setLogs([]);
        return;
      }

      setLoading(true);

      try {
        let response;

        if (type === 'employee') {
          response = await api.get(
            `/employees/${selectedId}/audit`
          );
        } else if (type === 'project') {
          response = await api.get(
            `/projects/${selectedId}/audit`
          );
        } else {
          response = await api.get(
            `/leads/${selectedId}/audit`
          );
        }

        setLogs(items(response.data));
      } catch (error) {
        console.error(
          'Failed to load audit logs',
          error
        );

        setLogs([]);
      } finally {
        setLoading(false);
      }
    }

    loadAudit();
  }, [type, selectedId]);

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
        {/* Audit type */}
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

        {/* Entity selector */}
        <select
          value={selectedId}
          onChange={(e) =>
            setSelectedId(e.target.value)
          }
          disabled={optionsLoading}
          className="min-w-[280px] rounded-lg border bg-white px-4 py-2.5 text-sm disabled:bg-slate-100"
        >
          <option value="">
            {optionsLoading
              ? 'Loading...'
              : `Select ${type}`}
          </option>

          {options.map((item: any) => (
            <option
              key={item.id}
              value={item.id}
            >
              {type === 'project'
                ? item.name
                : type === 'employee'
                  ? `${item.name}${item.email ? ` - ${item.email}` : ''}`
                  : `${item.leadName || item.name || '-'}${item.mobileNumber ? ` - ${item.mobileNumber}` : ''}`}
            </option>
          ))}
        </select>
      </div>

      <div className="overflow-hidden rounded-xl border bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-[1000px] w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-4 py-3">
                  Date
                </th>

                <th className="px-4 py-3">
                  User
                </th>

                <th className="px-4 py-3">
                  Action
                </th>

                <th className="px-4 py-3">
                  Old Value
                </th>

                <th className="px-4 py-3">
                  New Value
                </th>

                <th className="px-4 py-3">
                  Reason
                </th>
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

              {!loading &&
                !optionsLoading &&
                selectedId &&
                logs.length === 0 && (
                  <tr>
                    <td
                      colSpan={6}
                      className="px-4 py-8 text-center text-slate-500"
                    >
                      No audit records found.
                    </td>
                  </tr>
                )}

              {!loading &&
                !optionsLoading &&
                !selectedId && (
                  <tr>
                    <td
                      colSpan={6}
                      className="px-4 py-8 text-center text-slate-500"
                    >
                      Select a {type} to view audit history.
                    </td>
                  </tr>
                )}

              {!loading &&
                logs.map((log) => (
                  <tr key={log.id}>
                    <td className="whitespace-nowrap px-4 py-3">
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
