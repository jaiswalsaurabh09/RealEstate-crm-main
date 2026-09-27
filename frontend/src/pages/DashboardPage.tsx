import { useEffect, useState } from 'react';
import {
  Users,
  UserCheck,
  Building2,
  UserPlus,
  RefreshCw,
} from 'lucide-react';
import api from '../api/client';
import type { DashboardStats } from '../types/api';

function getStats(payload: any): DashboardStats {
  return payload?.data?.data || payload?.data || payload || {};
}

export default function DashboardPage() {
  const [stats, setStats] = useState<DashboardStats>({});
  const [loading, setLoading] = useState(true);

  async function load() {
    setLoading(true);

    try {
      const response = await api.get('/dashboard');
      setStats(getStats(response.data));
    } catch {
      try {
        const response = await api.get('/dashboard/stats');
        setStats(getStats(response.data));
      } catch {
        setStats({});
      }
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  const cards = [
    {
      title: 'Total Leads',
      value: stats.totalLeads ?? 0,
      icon: Users,
    },
    {
      title: 'New Leads',
      value: stats.newLeads ?? 0,
      icon: UserPlus,
    },
    {
      title: 'Assigned Leads',
      value: stats.assignedLeads ?? 0,
      icon: UserCheck,
    },
    {
      title: 'Converted Leads',
      value: stats.convertedLeads ?? 0,
      icon: RefreshCw,
    },
    {
      title: 'Active Projects',
      value: stats.activeProjects ?? 0,
      icon: Building2,
    },
    {
      title: 'Active Employees',
      value: stats.activeEmployees ?? 0,
      icon: Users,
    },
  ];

  return (
    <div>
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">
            Dashboard
          </h1>
          <p className="mt-1 text-sm text-slate-500">
            Silverland CRM overview
          </p>
        </div>

        <button
          onClick={load}
          className="flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium hover:bg-slate-50"
        >
          <RefreshCw size={16} />
          Refresh
        </button>
      </div>

      {loading ? (
        <div className="rounded-xl bg-white p-10 text-center text-slate-500">
          Loading dashboard...
        </div>
      ) : (
        <div className="grid gap-5 sm:grid-cols-2 xl:grid-cols-3">
          {cards.map((card) => {
            const Icon = card.icon;

            return (
              <div
                key={card.title}
                className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-slate-500">
                      {card.title}
                    </p>
                    <p className="mt-2 text-3xl font-bold text-slate-900">
                      {card.value}
                    </p>
                  </div>

                  <div className="rounded-xl bg-indigo-50 p-3 text-indigo-600">
                    <Icon size={24} />
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
