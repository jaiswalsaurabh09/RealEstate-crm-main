import { FormEvent, useEffect, useState } from 'react';
import { Plus, Pencil, Trash2, KeyRound } from 'lucide-react';
import api from '../api/client';
import type { Employee } from '../types/api';

function items(payload: any): any[] {
  const value = payload?.data?.data ?? payload?.data ?? payload;
  return Array.isArray(value)
    ? value
    : value?.items || value?.data || value?.results || [];
}

export default function EmployeesPage() {
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [show, setShow] = useState(false);
  const [editing, setEditing] = useState<Employee | null>(null);
  const [loading, setLoading] = useState(true);

  const [form, setForm] = useState({
    name: '',
    email: '',
    mobile: '',
    employeeCode: '',
    role: 'Employee',
    password: '',
  });

  async function load() {
    setLoading(true);

    try {
      const response = await api.get('/employees', {
        params: { page: 1, pageSize: 100 },
      });

      setEmployees(items(response.data));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  function openCreate() {
    setEditing(null);
    setForm({
      name: '',
      email: '',
      mobile: '',
      employeeCode: '',
      role: 'Employee',
      password: '',
    });
    setShow(true);
  }

  function openEdit(employee: Employee) {
    setEditing(employee);
    setForm({
      name: employee.name,
      email: employee.email,
      mobile: employee.mobile || '',
      employeeCode: employee.employeeCode || '',
      role: employee.role,
      password: '',
    });
    setShow(true);
  }

  async function save(e: FormEvent) {
    e.preventDefault();

    const body: any = {
      name: form.name,
      email: form.email,
      mobile: form.mobile,
      employeeCode: form.employeeCode,
      role: form.role,
    };

    if (form.password) body.password = form.password;

    if (editing) {
      await api.put(`/employees/${editing.id}`, body);
    } else {
      await api.post('/employees', body);
    }

    setShow(false);
    await load();
  }

  async function toggle(employee: Employee) {
    if (employee.isLoginEnabled) {
      await api.post(`/employees/${employee.id}/disable-login`);
    } else {
      await api.post(`/employees/${employee.id}/enable-login`);
    }

    await load();
  }

  async function remove(employee: Employee) {
    if (!confirm(`Delete ${employee.name}?`)) return;

    await api.delete(`/employees/${employee.id}`);
    await load();
  }

  async function resetPassword(employee: Employee) {
    const password = prompt(
      `Enter new password for ${employee.name}:`
    );

    if (!password) return;

    await api.post(
      `/employees/${employee.id}/reset-password`,
      { password }
    );

    alert('Password reset successfully.');
  }

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Employees</h1>
          <p className="text-sm text-slate-500">
            Manage CRM users and login access
          </p>
        </div>

        <button
          onClick={openCreate}
          className="flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white"
        >
          <Plus size={18} />
          Add Employee
        </button>
      </div>

      <div className="overflow-hidden rounded-xl border bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-[900px] w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-4 py-3">Employee</th>
                <th className="px-4 py-3">Code</th>
                <th className="px-4 py-3">Role</th>
                <th className="px-4 py-3">Login</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>

            <tbody className="divide-y">
              {loading ? (
                <tr>
                  <td colSpan={6} className="p-10 text-center">
                    Loading...
                  </td>
                </tr>
              ) : (
                employees.map((employee) => (
                  <tr key={employee.id}>
                    <td className="px-4 py-4">
                      <div className="font-semibold">
                        {employee.name}
                      </div>
                      <div className="text-xs text-slate-500">
                        {employee.email}
                      </div>
                    </td>

                    <td className="px-4 py-4">
                      {employee.employeeCode || '-'}
                    </td>

                    <td className="px-4 py-4">
                      {employee.role}
                    </td>

                    <td className="px-4 py-4">
                      <button
                        onClick={() => toggle(employee)}
                        className={`rounded-full px-3 py-1 text-xs font-medium ${
                          employee.isLoginEnabled
                            ? 'bg-green-100 text-green-700'
                            : 'bg-red-100 text-red-700'
                        }`}
                      >
                        {employee.isLoginEnabled
                          ? 'Enabled'
                          : 'Disabled'}
                      </button>
                    </td>

                    <td className="px-4 py-4">
                      {employee.isActive
                        ? 'Active'
                        : 'Inactive'}
                    </td>

                    <td className="px-4 py-4">
                      <div className="flex justify-end gap-1">
                        <button
                          onClick={() => openEdit(employee)}
                          className="rounded p-2 hover:bg-slate-100"
                        >
                          <Pencil size={17} />
                        </button>

                        <button
                          onClick={() =>
                            resetPassword(employee)
                          }
                          className="rounded p-2 text-indigo-600 hover:bg-indigo-50"
                        >
                          <KeyRound size={17} />
                        </button>

                        <button
                          onClick={() => remove(employee)}
                          className="rounded p-2 text-red-600 hover:bg-red-50"
                        >
                          <Trash2 size={17} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {show && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <form
            onSubmit={save}
            className="w-full max-w-lg rounded-xl bg-white p-6 shadow-2xl"
          >
            <h2 className="mb-5 text-xl font-bold">
              {editing ? 'Edit Employee' : 'Add Employee'}
            </h2>

            <div className="space-y-3">
              <input
                required
                placeholder="Name"
                value={form.name}
                onChange={(e) =>
                  setForm({ ...form, name: e.target.value })
                }
                className="w-full rounded-lg border p-3"
              />

              <input
                required
                type="email"
                placeholder="Email"
                value={form.email}
                onChange={(e) =>
                  setForm({
                    ...form,
                    email: e.target.value,
                  })
                }
                className="w-full rounded-lg border p-3"
              />

              <input
                placeholder="Mobile"
                value={form.mobile}
                onChange={(e) =>
                  setForm({
                    ...form,
                    mobile: e.target.value,
                  })
                }
                className="w-full rounded-lg border p-3"
              />

              <input
                placeholder="Employee Code"
                value={form.employeeCode}
                onChange={(e) =>
                  setForm({
                    ...form,
                    employeeCode: e.target.value,
                  })
                }
                className="w-full rounded-lg border p-3"
              />

              <select
                value={form.role}
                onChange={(e) =>
                  setForm({
                    ...form,
                    role: e.target.value,
                  })
                }
                className="w-full rounded-lg border p-3"
              >
                <option value="Employee">Employee</option>
                <option value="Admin">Admin</option>
              </select>

              {!editing && (
                <input
                  required
                  type="password"
                  placeholder="Password"
                  value={form.password}
                  onChange={(e) =>
                    setForm({
                      ...form,
                      password: e.target.value,
                    })
                  }
                  className="w-full rounded-lg border p-3"
                />
              )}
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={() => setShow(false)}
                className="rounded-lg border px-4 py-2"
              >
                Cancel
              </button>

              <button className="rounded-lg bg-indigo-600 px-5 py-2 font-semibold text-white">
                Save
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
}
