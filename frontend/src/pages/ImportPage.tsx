import { FormEvent, useState } from 'react';
import {
  Upload,
  FileSpreadsheet,
  CheckCircle,
  AlertCircle,
} from 'lucide-react';
import { importLeads } from '../api/importApi';

export default function ImportPage() {
  const [portalType, setPortalType] = useState('99acres');
  const [file, setFile] = useState<File | null>(null);
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<any>(null);
  const [error, setError] = useState('');

  async function submit(e: FormEvent) {
    e.preventDefault();

    if (!file) {
      setError('Please select a CSV or Excel file.');
      return;
    }

    setLoading(true);
    setError('');
    setResult(null);

    try {
      const response = await importLeads(file, portalType);
      setResult(response?.data?.data ?? response?.data ?? response);
      setFile(null);
    } catch (err: any) {
      setError(
        err?.response?.data?.message ||
        'Unable to import leads.'
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="mx-auto max-w-5xl">

      <div className="mb-6">
        <h1 className="text-2xl font-bold text-slate-900">
          Import Leads
        </h1>

        <p className="mt-1 text-sm text-slate-500">
          Import leads from different property portals.
        </p>
      </div>

      <form
        onSubmit={submit}
        className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
      >

        <div className="grid gap-5 md:grid-cols-2">

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">
              Portal Type
            </label>

            <select
              value={portalType}
              onChange={(e) => setPortalType(e.target.value)}
              className="w-full rounded-lg border border-slate-300 p-3 text-sm outline-none focus:border-indigo-500"
            >
              <option value="99acres">99acres</option>
              <option value="MagicBricks">MagicBricks</option>
              <option value="Housing.com">Housing.com</option>
              <option value="CommonFloor">CommonFloor</option>
              <option value="Facebook">Facebook</option>
              <option value="Website">Website</option>
              <option value="Generic">Generic / Other</option>
            </select>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-slate-700">
              File
            </label>

            <label className="flex cursor-pointer items-center gap-3 rounded-lg border-2 border-dashed border-slate-300 p-3 hover:border-indigo-400">
              <Upload size={20} className="text-indigo-600" />

              <span className="text-sm text-slate-600">
                {file
                  ? file.name
                  : 'Choose CSV or Excel file'}
              </span>

              <input
                type="file"
                accept=".csv,.xlsx,.xls"
                className="hidden"
                onChange={(e) =>
                  setFile(e.target.files?.[0] || null)
                }
              />
            </label>
          </div>

        </div>

        <div className="mt-5 rounded-lg bg-indigo-50 p-4 text-sm text-indigo-800">
          <strong>{portalType}</strong> format will be used
          for this import.
          <br />
          Existing leads with the same mobile number will be
          updated instead of creating another lead.
        </div>

        <div className="mt-6 flex justify-end">
          <button
            type="submit"
            disabled={!file || loading}
            className="flex items-center gap-2 rounded-lg bg-indigo-600 px-6 py-2.5 text-sm font-semibold text-white hover:bg-indigo-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <FileSpreadsheet size={18} />
            {loading ? 'Importing...' : 'Import Leads'}
          </button>
        </div>
      </form>

      {error && (
        <div className="mt-5 flex gap-3 rounded-lg bg-red-50 p-4 text-sm text-red-700">
          <AlertCircle size={20} />
          <span>{error}</span>
        </div>
      )}

      {result && (
        <div className="mt-5 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">

          <div className="mb-5 flex items-center gap-3">
            <CheckCircle
              size={24}
              className="text-green-600"
            />

            <div>
              <h2 className="font-bold text-slate-900">
                Import Completed
              </h2>

              <p className="text-sm text-slate-500">
                {result.fileName}
              </p>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3 md:grid-cols-5">

            <Stat
              label="Total"
              value={result.totalRows ?? 0}
            />

            <Stat
              label="Created"
              value={result.createdRows ?? 0}
            />

            <Stat
              label="Updated"
              value={result.updatedRows ?? 0}
            />

            <Stat
              label="Duplicate"
              value={result.duplicateRows ?? 0}
            />

            <Stat
              label="Failed"
              value={result.failedRows ?? 0}
            />

          </div>

          {Array.isArray(result.errors) &&
            result.errors.length > 0 && (
              <div className="mt-5 rounded-lg bg-red-50 p-4">
                <h3 className="mb-2 font-semibold text-red-800">
                  Import Errors
                </h3>

                <div className="max-h-60 overflow-y-auto text-xs text-red-700">
                  {result.errors.map(
                    (item: string, index: number) => (
                      <div key={index} className="mb-1">
                        {item}
                      </div>
                    )
                  )}
                </div>
              </div>
            )}

        </div>
      )}
    </div>
  );
}

function Stat({
  label,
  value,
}: {
  label: string;
  value: number;
}) {
  return (
    <div className="rounded-lg bg-slate-50 p-4 text-center">
      <div className="text-2xl font-bold text-slate-900">
        {value}
      </div>

      <div className="text-xs text-slate-500">
        {label}
      </div>
    </div>
  );
}
