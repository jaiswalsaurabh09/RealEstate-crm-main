import { NavLink, useNavigate } from 'react-router-dom';
import {
  BarChart3,
  Building2,
  FileSpreadsheet,
  LayoutDashboard,
  LogOut,
  Users,
  ClipboardList,
} from 'lucide-react';
import { useAuth } from '../contexts/AuthContext';

export default function Sidebar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const linkClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition ${
      isActive
        ? 'bg-indigo-600 text-white'
        : 'text-slate-300 hover:bg-slate-800 hover:text-white'
    }`;

  const logoutUser = () => {
    logout();
    navigate('/login');
  };

  return (
    <aside className="fixed inset-y-0 left-0 z-40 flex w-64 flex-col bg-slate-950 text-white">
      <div className="border-b border-slate-800 px-5 py-5">
        <div className="text-xl font-bold tracking-wide text-white">
          SILVERLAND
        </div>
        <div className="text-xs font-medium tracking-widest text-indigo-400">
          CRM
        </div>
      </div>

      <nav className="flex-1 space-y-1 overflow-y-auto p-4">
        <NavLink to="/dashboard" className={linkClass}>
          <LayoutDashboard size={18} />
          Dashboard
        </NavLink>

        <NavLink to="/leads" className={linkClass}>
          <Users size={18} />
          Master Leads
        </NavLink>

        {user?.role === 'Admin' && (
          <>
            <NavLink to="/projects" className={linkClass}>
              <Building2 size={18} />
              Projects
            </NavLink>

            <NavLink to="/employees" className={linkClass}>
              <Users size={18} />
              Employees
            </NavLink>

            <NavLink to="/import" className={linkClass}>
              <FileSpreadsheet size={18} />
              Import Leads
            </NavLink>

            <NavLink to="/audit" className={linkClass}>
              <ClipboardList size={18} />
              Audit Logs
            </NavLink>
          </>
        )}
      </nav>

      <div className="border-t border-slate-800 p-4">
        <div className="mb-3">
          <div className="truncate text-sm font-semibold">
            {user?.name}
          </div>
          <div className="truncate text-xs text-slate-400">
            {user?.email}
          </div>
          <div className="mt-1 inline-block rounded bg-indigo-500/20 px-2 py-0.5 text-xs text-indigo-300">
            {user?.role}
          </div>
        </div>

        <button
          onClick={logoutUser}
          className="flex w-full items-center justify-center gap-2 rounded-lg bg-red-600 px-3 py-2 text-sm font-medium hover:bg-red-700"
        >
          <LogOut size={16} />
          Logout
        </button>
      </div>
    </aside>
  );
}
