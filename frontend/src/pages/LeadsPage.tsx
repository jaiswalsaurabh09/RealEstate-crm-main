import { FormEvent, useEffect, useState } from 'react';
import {
  Plus,
  Search,
  Pencil,
  Trash2,
  UserRoundPlus,
  X,
} from 'lucide-react';
import api from '../api/client';
import {
  createLead,
  deleteLead,
  getLeads,
  updateLead,
  assignLead,
} from '../api/leadsApi';
import { useAuth } from '../contexts/AuthContext';
import type { Employee, Lead, Project } from '../types/api';

function unwrap(payload: any): any {
  return payload?.data?.data ?? payload?.data ?? payload;
}

function items(payload: any): any[] {
  const value = unwrap(payload);

  if (Array.isArray(value)) return value;

  return (
    value?.items ||
    value?.data ||
    value?.results ||
    []
  );
}

export default function LeadsPage() {
  const { user } = useAuth();

  const isAdmin = user?.role === 'Admin';
  const isEmployee = user?.role === 'Employee';

  const [leads, setLeads] = useState<Lead[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [employees, setEmployees] = useState<Employee[]>([]);

  const [search, setSearch] = useState('');
  const [responseFilter, setResponseFilter] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<Lead | null>(null);
  const [assigning, setAssigning] = useState<Lead | null>(null);

  const [page, setPage] = useState(1);
  const pageSize = 20;

  const [form, setForm] = useState({
    leadName: '',
    mobileNumber: '',
    email: '',
    enquiredOn: '',
    response: 'New',
    responseDetails: '',
    projectId: '',
    leadSource: 'Manual',
    assignedEmployeeId: '',
  });

  async function load() {
    setLoading(true);
    setError('');

    try {
      const response = await getLeads({
        page,
        pageSize,
        search: search || undefined,
        response: responseFilter || undefined,
      });

      setLeads(items(response));
    } catch (err: any) {
      setError(
        err?.response?.data?.message ||
          'Unable to load leads.'
      );
    } finally {
      setLoading(false);
    }
  }

  /*
   * Load projects independently.
   * A failed employee API must not prevent projects
   * from appearing.
   */
  async function loadProjects() {
    try {
      const response = await api.get('/projects', {
        params: {
          includeInactive: false,
        },
      });

      setProjects(items(response));
    } catch (err) {
      console.error('Unable to load projects', err);
      setProjects([]);
    }
  }

  /*
   * Employees are only needed by Admin.
   * Employee users don't need the employee list.
   */
  async function loadEmployees() {
    if (!isAdmin) {
      setEmployees([]);
      return;
    }

    try {
      const response = await api.get('/employees', {
        params: {
          page: 1,
          pageSize: 100,
        },
      });

      setEmployees(items(response));
    } catch (err) {
      console.error('Unable to load employees', err);
      setEmployees([]);
    }
  }

  useEffect(() => {
    load();
  }, [page, responseFilter]);

  useEffect(() => {
    loadProjects();
    loadEmployees();
  }, [isAdmin]);

  function openCreate() {
    setEditing(null);

    setForm({
      leadName: '',
      mobileNumber: '',
      email: '',
      enquiredOn: '',
      response: 'New',
      responseDetails: '',
      projectId: '',
      leadSource: 'Manual',

      /*
       * Employee automatically owns the lead.
       * Admin starts with Unassigned.
       */
      assignedEmployeeId:
        isEmployee && user?.id
          ? user.id
          : '',
    });

    setShowForm(true);
  }

  function openEdit(lead: Lead) {
    setEditing(lead);

    setForm({
      leadName: lead.leadName || '',
      mobileNumber: lead.mobileNumber || '',
      email: lead.email || '',
      enquiredOn: lead.enquiredOn || '',
      response: lead.response || 'New',
      responseDetails: lead.responseDetails || '',
      projectId: lead.projectId || '',
      leadSource: lead.leadSource || 'Manual',

      /*
       * Employee cannot change ownership.
       * Always use the logged-in employee.
       */
      assignedEmployeeId:
        isEmployee && user?.id
          ? user.id
          : lead.assignedEmployeeId || '',
    });

    setShowForm(true);
  }

  async function save(e: FormEvent) {
    e.preventDefault();

    try {
      /*
       * Employee assignment is always forced to
       * the logged-in employee.
       *
       * Admin can choose employee or Unassigned.
       */
      const assignedEmployeeId = isEmployee
        ? user?.id
        : form.assignedEmployeeId || undefined;

      const payload = {
        ...form,
        projectId: form.projectId || undefined,
        assignedEmployeeId,
      };

      if (editing) {
        await updateLead(editing.id, payload);
      } else {
        await createLead(payload);
      }

      setShowForm(false);
      await load();
    } catch (err: any) {
      alert(
        err?.response?.data?.message ||
          'Unable to save lead.'
      );
    }
  }

  async function remove(id: string) {
    if (!confirm('Delete this lead?')) return;

    try {
      await deleteLead(id);
      await load();
    } catch (err: any) {
      alert(
        err?.response?.data?.message ||
          'Unable to delete lead.'
      );
    }
  }

  async function saveAssignment(employeeId: string) {
    if (!assigning || !isAdmin) return;

    try {
      await assignLead(
        assigning.id,
        employeeId || undefined
      );

      setAssigning(null);
      await load();
    } catch (err: any) {
      alert(
        err?.response?.data?.message ||
          'Unable to assign lead.'
      );
    }
  }

  return (
    <div>
      <div className="mb-6 flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">
            Master Leads
          </h1>

          <p className="text-sm text-slate-500">
            Manage and track all enquiries
          </p>
        </div>

        <button
          onClick={openCreate}
          className="flex items-center justify-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white hover:bg-indigo-700"
        >
          <Plus size={18} />
          Add Lead
        </button>
      </div>

      <div className="mb-5 rounded-xl border border-slate-200 bg-white p-4">
        <div className="flex flex-col gap-3 md:flex-row">
          <div className="relative flex-1">
            <Search
              size={18}
              className="absolute left-3 top-3 text-slate-400"
            />

            <input
              value={search}
              onChange={(e) =>
                setSearch(e.target.value)
              }
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  setPage(1);
                  load();
                }
              }}
              placeholder="Search name, mobile, email..."
              className="w-full rounded-lg border border-slate-300 py-2.5 pl-10 pr-3 text-sm outline-none focus:border-indigo-500"
            />
          </div>

          <select
            value={responseFilter}
            onChange={(e) => {
              setResponseFilter(e.target.value);
              setPage(1);
            }}
            className="rounded-lg border border-slate-300 px-3 py-2.5 text-sm"
          >
            <option value="">All Responses</option>
            <option value="New">New</option>
            <option value="Contacted">Contacted</option>
            <option value="Interested">Interested</option>
            <option value="Follow Up">Follow Up</option>
            <option value="Converted">Converted</option>
            <option value="Not Interested">Not Interested</option>
          </select>

          <button
            onClick={() => {
              setPage(1);
              load();
            }}
            className="rounded-lg bg-slate-900 px-5 py-2.5 text-sm font-medium text-white hover:bg-slate-800"
          >
            Search
          </button>
        </div>
      </div>

      {error && (
        <div className="mb-5 rounded-lg bg-red-50 p-4 text-sm text-red-700">
          {error}
        </div>
      )}

      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-[1200px] w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-4 py-3">Lead</th>
                <th className="px-4 py-3">Mobile</th>
                <th className="px-4 py-3">Project</th>
                <th className="px-4 py-3">Enquired On</th>
                <th className="px-4 py-3">Response</th>
                <th className="px-4 py-3">Assigned</th>
                <th className="px-4 py-3">Source</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>

            <tbody className="divide-y divide-slate-100">
              {loading ? (
                <tr>
                  <td
                    colSpan={8}
                    className="px-4 py-12 text-center text-slate-500"
                  >
                    Loading leads...
                  </td>
                </tr>
              ) : leads.length === 0 ? (
                <tr>
                  <td
                    colSpan={8}
                    className="px-4 py-12 text-center text-slate-500"
                  >
                    No leads found.
                  </td>
                </tr>
              ) : (
                leads.map((lead) => (
                  <tr
                    key={lead.id}
                    className="hover:bg-slate-50"
                  >
                    <td className="px-4 py-3">
                      <div className="font-semibold text-slate-900">
                        {lead.leadName}
                      </div>

                      <div className="text-xs text-slate-500">
                        {lead.email || '-'}
                      </div>
                    </td>

                    <td className="px-4 py-3">
                      {lead.mobileNumber}
                    </td>

                    <td className="px-4 py-3">
                      {lead.projectName || '-'}
                    </td>

                    <td className="px-4 py-3">
                      {lead.enquiredOn || '-'}
                    </td>

                    <td className="px-4 py-3">
                      <span className="rounded-full bg-indigo-50 px-2.5 py-1 text-xs font-medium text-indigo-700">
                        {lead.response}
                      </span>
                    </td>

                    <td className="px-4 py-3">
                      {lead.assignedEmployeeName ||
                        'Unassigned'}
                    </td>

                    <td className="px-4 py-3">
                      {lead.leadSource}
                    </td>

                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-1">

                        {/* ASSIGN IS ADMIN ONLY */}
                        {isAdmin && (
                          <button
                            onClick={() =>
                              setAssigning(lead)
                            }
                            title="Assign"
                            className="rounded p-2 text-indigo-600 hover:bg-indigo-50"
                          >
                            <UserRoundPlus size={17} />
                          </button>
                        )}

                        <button
                          onClick={() =>
                            openEdit(lead)
                          }
                          title="Edit"
                          className="rounded p-2 text-slate-600 hover:bg-slate-100"
                        >
                          <Pencil size={17} />
                        </button>

                        {isAdmin && (
                          <button
                            onClick={() =>
                              remove(lead.id)
                            }
                            title="Delete"
                            className="rounded p-2 text-red-600 hover:bg-red-50"
                          >
                            <Trash2 size={17} />
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        <div className="flex items-center justify-between border-t border-slate-200 px-4 py-3">
          <span className="text-xs text-slate-500">
            Page {page}
          </span>

          <div className="flex gap-2">
            <button
              disabled={page <= 1}
              onClick={() =>
                setPage((p) => p - 1)
              }
              className="rounded border px-3 py-1.5 text-sm disabled:opacity-40"
            >
              Previous
            </button>

            <button
              disabled={leads.length < pageSize}
              onClick={() =>
                setPage((p) => p + 1)
              }
              className="rounded border px-3 py-1.5 text-sm disabled:opacity-40"
            >
              Next
            </button>
          </div>
        </div>
      </div>

      {/* CREATE / EDIT LEAD */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-xl bg-white shadow-2xl">

            <div className="flex items-center justify-between border-b p-5">
              <h2 className="text-lg font-bold">
                {editing
                  ? 'Edit Lead'
                  : 'Add Lead'}
              </h2>

              <button
                onClick={() =>
                  setShowForm(false)
                }
                className="rounded p-2 hover:bg-slate-100"
              >
                <X size={20} />
              </button>
            </div>

            <form
              onSubmit={save}
              className="space-y-4 p-5"
            >
              <div className="grid gap-4 md:grid-cols-2">

                <input
                  placeholder="Lead Name *"
                  required
                  value={form.leadName}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      leadName: e.target.value,
                    })
                  }
                  className="rounded-lg border p-2.5"
                />

                <input
                  placeholder="Mobile Number *"
                  required
                  value={form.mobileNumber}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      mobileNumber: e.target.value,
                    })
                  }
                  className="rounded-lg border p-2.5"
                />

                <input
                  type="email"
                  placeholder="Email"
                  value={form.email}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      email: e.target.value,
                    })
                  }
                  className="rounded-lg border p-2.5"
                />

                <input
                  placeholder="Enquired On"
                  value={form.enquiredOn}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      enquiredOn: e.target.value,
                    })
                  }
                  className="rounded-lg border p-2.5"
                />

                <select
                  value={form.response}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      response: e.target.value,
                    })
                  }
                  className="rounded-lg border p-2.5"
                >
                  <option>New</option>
                  <option>Contacted</option>
                  <option>Interested</option>
                  <option>Follow Up</option>
                  <option>Converted</option>
                  <option>Not Interested</option>
                </select>

                <select
                  value={form.projectId}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      projectId: e.target.value,
                    })
                  }
                  className="rounded-lg border p-2.5"
                >
                  <option value="">
                    Select Project
                  </option>

                  {projects.map((project) => (
                    <option
                      key={project.id}
                      value={project.id}
                    >
                      {project.name}
                    </option>
                  ))}
                </select>

                {/* ADMIN: Assignment dropdown */}
                {isAdmin && (
                  <select
                    value={form.assignedEmployeeId}
                    onChange={(e) =>
                      setForm({
                        ...form,
                        assignedEmployeeId:
                          e.target.value,
                      })
                    }
                    className="rounded-lg border p-2.5"
                  >
                    <option value="">
                      Unassigned
                    </option>

                    {employees.map(
                      (employee) => (
                        <option
                          key={employee.id}
                          value={employee.id}
                        >
                          {employee.name}
                        </option>
                      )
                    )}
                  </select>
                )}

                {/* EMPLOYEE: Read-only assignment */}
                {isEmployee && (
                  <div>
                    <label className="mb-1 block text-sm font-medium text-slate-700">
                      Assigned Employee
                    </label>

                    <input
                      value={user?.name || ''}
                      disabled
                      readOnly
                      className="w-full rounded-lg border bg-slate-100 p-2.5 text-slate-600"
                    />

                    <p className="mt-1 text-xs text-slate-500">
                      This lead is automatically assigned
                      to you.
                    </p>
                  </div>
                )}

                <input
                  placeholder="Lead Source"
                  value={form.leadSource}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      leadSource: e.target.value,
                    })
                  }
                  className="rounded-lg border p-2.5"
                />
              </div>

              <textarea
                placeholder="Response Details"
                value={form.responseDetails}
                onChange={(e) =>
                  setForm({
                    ...form,
                    responseDetails:
                      e.target.value,
                  })
                }
                rows={3}
                className="w-full rounded-lg border p-2.5"
              />

              <div className="flex justify-end gap-3">
                <button
                  type="button"
                  onClick={() =>
                    setShowForm(false)
                  }
                  className="rounded-lg border px-4 py-2 text-sm"
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  className="rounded-lg bg-indigo-600 px-5 py-2 text-sm font-semibold text-white hover:bg-indigo-700"
                >
                  Save Lead
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* ADMIN ONLY: ASSIGN LEAD */}
      {assigning && isAdmin && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-2xl">

            <h2 className="mb-1 text-lg font-bold">
              Assign Lead
            </h2>

            <p className="mb-5 text-sm text-slate-500">
              {assigning.leadName}
            </p>

            <select
              defaultValue={
                assigning.assignedEmployeeId || ''
              }
              onChange={(e) =>
                saveAssignment(
                  e.target.value
                )
              }
              className="mb-4 w-full rounded-lg border p-3"
            >
              <option value="">
                Unassign
              </option>

              {employees.map((employee) => (
                <option
                  key={employee.id}
                  value={employee.id}
                >
                  {employee.name}
                </option>
              ))}
            </select>

            <button
              onClick={() =>
                setAssigning(null)
              }
              className="w-full rounded-lg border py-2"
            >
              Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
