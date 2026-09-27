import { useEffect, useState } from 'react';
import { Upload, FileSpreadsheet } from 'lucide-react';
import { getImportHistory, importLeads } from '../api/importApi';
import type { ImportHistory } from '../types/api';

function items(payload: any): any[] {
  const value = payload?.data?.data ?? payload?.data ?? payload;
  return Array.isArray(value)
    ? value
    : value?.items || value?.data || value?.results || [];
}

export default function ImportPage() {
  const [file, setFile] = useState<File | null>(null);
  const [history, setHistory] = useState<ImportHistory[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  async function loadHistory() {
    try {
      const response = await getImportHistory();
      setHistory(items(response));
    } catch {
      setHistory([]);
    }
  }

  useEffect(() => {
    loadHistory();
  }, []);

  async function upload() {
    if (!file) {
      setMessage('Please select an Excel file.');
      return;
    }

    if (!file.name.toLowerCase().endsWith('.xlsx')) {
      setMessage('Only .xlsx files are supported.');
      return;
    }

    setLoading(true);
    setMessage('');

    try {
      await importLeads(file);
      setMessage('Import completed successfully.');
      setFile(null);
      await loadHistory();
    } catch (err: any) {
      setMessage(
        err?.response?.data?.message ||
          'Import failed.'
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold">
          Import Leads
        </h1>
        <p className="text-sm text-slate-500">
          Import leads from an Excel workbook
        </p>
      </div>

      <div className="rounded-xl border bg-white p-6 shadow-sm">
        <div className="flex flex-col items-center justify-center rounded-xl border-2 border-dashed border-slate-300 p-10">
          <FileSpreadsheet
            size={42}
            className="mb-3 text-indigo-600"
          />

          <h2 className="font-semibold">
            Select Excel file
          </h2>

          <p className="mb-5 text-sm text-slate-500">
            Supported format: .xlsx, maximum 25 MB
          </p>

          <input
            id="excel"
            type="file"
            accept=".xlsx"
            className="hidden"
            onChange={(e) =>
              setFile(e.target.files?.[0] || null)
            }
          />

          <label
            htmlFor="excel"
            className="cursor-pointer rounded-lg border bg-white px-4 py-2 text-sm font-medium hover:bg-slate-50"
          >
            Choose File
          </label>

          {file && (
            <div className="mt-4 text-sm font-medium text-slate-700">
              {file.name}
            </div>
          )}

          <button
            onClick={upload}
            disabled={!file || loading}
            className="mt-5 flex items-center gap-2 rounded-lg bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white disabled:opacity-50"
          >
            <Upload size={17} />
            {loading ? 'Importing...' : 'Import Leads'}
          </button>

          {message && (
            <div className="mt-4 rounded-lg bg-slate-100 px-4 py-3 text-sm">
              {message}
            </div>
          )}
        </div>
      </div>

      <div className="mt-6 rounded-xl border bg-white shadow-sm">
        <div className="border-b p-5">
          <h2 className="font-bold">Import History</h2>
        </div>

        <div className="overflow-x-auto">
          <table className="min-w-[800px] w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase text-slate-500">
              <tr>
                <th className="px-4 py-3">File</th>
                <th className="px-4 py-3">Total</th>
                <th className="px-4 py-3">Inserted</th>
                <th className="px-4 py-3">Updated</th>
                <th className="px-4 py-3">Failed</th>
                <th className="px-4 py-3">Date</th>
              </tr>
            </thead>

            <tbody className="divide-y">
              {history.map((row) => (
                <tr key={row.id}>
                  <td className="px-4 py-3">
                    {row.fileName || '-'}
                  </td>
                  <td className="px-4 py-3">
                    {row.totalRows ?? 0}
                  </td>
                  <td className="px-4 py-3">
                    {row.insertedRows ?? 0}
                  </td>
                  <td className="px-4 py-3">
                    {row.updatedRows ?? 0}
                  </td>
                  <td className="px-4 py-3">
                    {row.failedRows ?? 0}
                  </td>
                  <td className="px-4 py-3">
                    {row.importedAt
                      ? new Date(
                          row.importedAt
                        ).toLocaleString()
                      : '-'}
                  </td>
                </tr>
              ))}

              {history.length === 0 && (
                <tr>
                  <td
                    colSpan={6}
                    className="p-8 text-center text-slate-500"
                  >
                    No import history found.
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
