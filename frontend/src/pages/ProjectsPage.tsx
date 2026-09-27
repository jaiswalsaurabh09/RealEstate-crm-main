import { FormEvent, useEffect, useState } from 'react';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import api from '../api/client';
import type { Project } from '../types/api';

function items(payload: any): any[] {
  const value = payload?.data?.data ?? payload?.data ?? payload;
  return Array.isArray(value)
    ? value
    : value?.items || value?.data || value?.results || [];
}

export default function ProjectsPage() {
  const [projects, setProjects] = useState<Project[]>([]);
  const [show, setShow] = useState(false);
  const [editing, setEditing] = useState<Project | null>(null);

  const [form, setForm] = useState({
    name: '',
    location: '',
    description: '',
  });

  async function load() {
    const response = await api.get('/projects');
    setProjects(items(response.data));
  }

  useEffect(() => {
    load();
  }, []);

  function create() {
    setEditing(null);
    setForm({
      name: '',
      location: '',
      description: '',
    });
    setShow(true);
  }

  function edit(project: Project) {
    setEditing(project);
    setForm({
      name: project.name,
      location: project.location,
      description: project.description || '',
    });
    setShow(true);
  }

  async function save(e: FormEvent) {
    e.preventDefault();

    if (editing) {
      await api.put(`/projects/${editing.id}`, form);
    } else {
      await api.post('/projects', form);
    }

    setShow(false);
    await load();
  }

  async function remove(project: Project) {
    if (!confirm(`Delete ${project.name}?`)) return;

    await api.delete(`/projects/${project.id}`);
    await load();
  }

  async function toggle(project: Project) {
    await api.post(
      `/projects/${project.id}/${
        project.isActive ? 'deactivate' : 'activate'
      }`
    );

    await load();
  }

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Projects</h1>
          <p className="text-sm text-slate-500">
            Manage real estate projects
          </p>
        </div>

        <button
          onClick={create}
          className="flex items-center gap-2 rounded-lg bg-indigo-600 px-4 py-2.5 text-sm font-semibold text-white"
        >
          <Plus size={18} />
          Add Project
        </button>
      </div>

      <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
        {projects.map((project) => (
          <div
            key={project.id}
            className="rounded-xl border bg-white p-5 shadow-sm"
          >
            <div className="flex items-start justify-between">
              <div>
                <h2 className="font-bold text-slate-900">
                  {project.name}
                </h2>
                <p className="mt-1 text-sm text-slate-500">
                  {project.location}
                </p>
              </div>

              <span
                className={`rounded-full px-2 py-1 text-xs ${
                  project.isActive
                    ? 'bg-green-100 text-green-700'
                    : 'bg-red-100 text-red-700'
                }`}
              >
                {project.isActive ? 'Active' : 'Inactive'}
              </span>
            </div>

            <p className="mt-4 min-h-12 text-sm text-slate-600">
              {project.description || 'No description'}
            </p>

            <div className="mt-5 flex justify-end gap-2 border-t pt-4">
              <button
                onClick={() => toggle(project)}
                className="rounded-lg border px-3 py-2 text-xs"
              >
                {project.isActive ? 'Deactivate' : 'Activate'}
              </button>

              <button
                onClick={() => edit(project)}
                className="rounded-lg p-2 hover:bg-slate-100"
              >
                <Pencil size={16} />
              </button>

              <button
                onClick={() => remove(project)}
                className="rounded-lg p-2 text-red-600 hover:bg-red-50"
              >
                <Trash2 size={16} />
              </button>
            </div>
          </div>
        ))}
      </div>

      {show && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
          <form
            onSubmit={save}
            className="w-full max-w-lg rounded-xl bg-white p-6"
          >
            <h2 className="mb-5 text-xl font-bold">
              {editing ? 'Edit Project' : 'Add Project'}
            </h2>

            <div className="space-y-3">
              <input
                required
                placeholder="Project Name"
                value={form.name}
                onChange={(e) =>
                  setForm({ ...form, name: e.target.value })
                }
                className="w-full rounded-lg border p-3"
              />

              <input
                required
                placeholder="Location"
                value={form.location}
                onChange={(e) =>
                  setForm({
                    ...form,
                    location: e.target.value,
                  })
                }
                className="w-full rounded-lg border p-3"
              />

              <textarea
                placeholder="Description"
                rows={4}
                value={form.description}
                onChange={(e) =>
                  setForm({
                    ...form,
                    description: e.target.value,
                  })
                }
                className="w-full rounded-lg border p-3"
              />
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
