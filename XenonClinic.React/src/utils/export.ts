/**
 * Export utilities for converting data to various formats
 */

export interface ExportColumn {
  key: string;
  label: string;
  format?: (value: unknown) => string;
}

/**
 * Export data to CSV format
 */
export const exportToCSV = (
  data: Record<string, unknown>[],
  columns: ExportColumn[],
  filename: string
) => {
  if (!data || data.length === 0) {
    throw new Error('No data to export');
  }

  // Create CSV header
  const headers = columns.map((col) => col.label).join(',');

  // Create CSV rows
  const rows = data.map((row) => {
    return columns
      .map((col) => {
        let value = row[col.key];
        if (col.format) {
          value = col.format(value);
        }
        // Escape quotes and wrap in quotes if contains comma or quotes
        if (value === null || value === undefined) {
          return '';
        }
        const stringValue = String(value);
        if (stringValue.includes(',') || stringValue.includes('"') || stringValue.includes('\n')) {
          return `"${stringValue.replace(/"/g, '""')}"`;
        }
        return stringValue;
      })
      .join(',');
  });

  // Combine header and rows
  const csv = [headers, ...rows].join('\n');

  // Create and trigger download
  downloadFile(csv, filename, 'text/csv;charset=utf-8;');
};

/**
 * Export data to Excel format (using HTML table format that Excel can read)
 */
export const exportToExcel = (
  data: Record<string, unknown>[],
  columns: ExportColumn[],
  filename: string
) => {
  if (!data || data.length === 0) {
    throw new Error('No data to export');
  }

  // Create HTML table
  let html = '<html><head><meta charset="utf-8"></head><body><table>';

  // Add header
  html += '<thead><tr>';
  columns.forEach((col) => {
    html += `<th>${col.label}</th>`;
  });
  html += '</tr></thead>';

  // Add rows
  html += '<tbody>';
  data.forEach((row) => {
    html += '<tr>';
    columns.forEach((col) => {
      let value = row[col.key];
      if (col.format) {
        value = col.format(value);
      }
      html += `<td>${value !== null && value !== undefined ? value : ''}</td>`;
    });
    html += '</tr>';
  });
  html += '</tbody></table></body></html>';

  // Create and trigger download
  downloadFile(html, filename, 'application/vnd.ms-excel');
};

/**
 * Export data to JSON format
 */
export const exportToJSON = (
  data: Record<string, unknown>[],
  filename: string
) => {
  if (!data || data.length === 0) {
    throw new Error('No data to export');
  }

  const json = JSON.stringify(data, null, 2);
  downloadFile(json, filename, 'application/json;charset=utf-8;');
};

/**
 * Print data in a formatted table
 */
export const printTable = (
  data: Record<string, unknown>[],
  columns: ExportColumn[],
  title?: string
) => {
  if (!data || data.length === 0) {
    throw new Error('No data to print');
  }

  // Create print-friendly HTML
  let html = `
    <!DOCTYPE html>
    <html>
    <head>
      <meta charset="utf-8">
      <title>${title || 'Print'}</title>
      <style>
        body { font-family: Arial, sans-serif; padding: 20px; }
        h1 { font-size: 24px; margin-bottom: 20px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f3f4f6; font-weight: bold; }
        tr:nth-child(even) { background-color: #f9fafb; }
        @media print {
          body { padding: 0; }
        }
      </style>
    </head>
    <body>
  `;

  if (title) {
    html += `<h1>${title}</h1>`;
  }

  html += '<table>';

  // Add header
  html += '<thead><tr>';
  columns.forEach((col) => {
    html += `<th>${col.label}</th>`;
  });
  html += '</tr></thead>';

  // Add rows
  html += '<tbody>';
  data.forEach((row) => {
    html += '<tr>';
    columns.forEach((col) => {
      let value = row[col.key];
      if (col.format) {
        value = col.format(value);
      }
      html += `<td>${value !== null && value !== undefined ? value : ''}</td>`;
    });
    html += '</tr>';
  });
  html += '</tbody></table></body></html>';

  // Open in new window and print
  const printWindow = window.open('', '_blank');
  if (printWindow) {
    printWindow.document.write(html);
    printWindow.document.close();
    printWindow.focus();
    setTimeout(() => {
      printWindow.print();
      printWindow.close();
    }, 250);
  }
};

/**
 * Helper function to trigger file download
 */
const downloadFile = (content: string, filename: string, mimeType: string) => {
  const blob = new Blob([content], { type: mimeType });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
};

/**
 * Format helpers for common data types
 */
export const formatters = {
  date: (value: unknown) => {
    if (!value) return '';
    const date = new Date(value as string | number | Date);
    return date.toLocaleDateString();
  },
  datetime: (value: unknown) => {
    if (!value) return '';
    const date = new Date(value as string | number | Date);
    return date.toLocaleString();
  },
  currency: (value: unknown, currency = 'AED') => {
    if (value === null || value === undefined) return '';
    return `${Number(value).toFixed(2)} ${currency}`;
  },
  boolean: (value: unknown) => {
    return value ? 'Yes' : 'No';
  },
};
