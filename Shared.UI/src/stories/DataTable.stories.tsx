import type { Meta, StoryObj } from '@storybook/react';
import { useState } from 'react';
import { DataTable, useDataTable, Column, SortState } from '../components/DataTable/DataTable';

interface Patient {
  id: number;
  name: string;
  email: string;
  status: 'active' | 'inactive';
  lastVisit: string;
}

const sampleData: Patient[] = [
  { id: 1, name: 'John Smith', email: 'john@example.com', status: 'active', lastVisit: '2024-01-15' },
  { id: 2, name: 'Jane Doe', email: 'jane@example.com', status: 'active', lastVisit: '2024-01-10' },
  { id: 3, name: 'Bob Johnson', email: 'bob@example.com', status: 'inactive', lastVisit: '2023-12-20' },
  { id: 4, name: 'Alice Brown', email: 'alice@example.com', status: 'active', lastVisit: '2024-01-12' },
  { id: 5, name: 'Charlie Wilson', email: 'charlie@example.com', status: 'inactive', lastVisit: '2023-11-30' },
];

const columns: Column<Patient>[] = [
  { key: 'name', header: 'Name', sortable: true },
  { key: 'email', header: 'Email', sortable: true },
  {
    key: 'status',
    header: 'Status',
    sortable: true,
    render: (item) => (
      <span
        className={`px-2 py-1 text-xs font-medium rounded-full ${
          item.status === 'active'
            ? 'bg-green-100 text-green-800'
            : 'bg-gray-100 text-gray-800'
        }`}
      >
        {item.status}
      </span>
    ),
  },
  { key: 'lastVisit', header: 'Last Visit', sortable: true, align: 'right' },
];

const meta: Meta<typeof DataTable> = {
  title: 'Components/DataTable',
  component: DataTable,
  parameters: {
    layout: 'padded',
  },
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof DataTable<Patient>>;

export const Default: Story = {
  render: () => (
    <DataTable
      data={sampleData}
      columns={columns}
      keyExtractor={(item) => item.id}
    />
  ),
};

export const Loading: Story = {
  render: () => (
    <DataTable
      data={[]}
      columns={columns}
      keyExtractor={(item) => item.id}
      isLoading={true}
    />
  ),
};

export const Empty: Story = {
  render: () => (
    <DataTable
      data={[]}
      columns={columns}
      keyExtractor={(item) => item.id}
      emptyTitle="No patients found"
      emptyDescription="Try adjusting your search criteria."
      emptyAction={{
        label: 'Add Patient',
        onClick: () => alert('Add patient clicked'),
      }}
    />
  ),
};

export const WithSorting: Story = {
  render: () => {
    const [sortState, setSortState] = useState<SortState>({ column: 'name', direction: 'asc' });

    const handleSort = (column: string) => {
      setSortState((prev) => {
        if (prev.column !== column) return { column, direction: 'asc' };
        if (prev.direction === 'asc') return { column, direction: 'desc' };
        return { column: null, direction: null };
      });
    };

    const sortedData = [...sampleData].sort((a, b) => {
      if (!sortState.column || !sortState.direction) return 0;
      const aVal = String(a[sortState.column as keyof Patient]);
      const bVal = String(b[sortState.column as keyof Patient]);
      const comparison = aVal.localeCompare(bVal);
      return sortState.direction === 'asc' ? comparison : -comparison;
    });

    return (
      <DataTable
        data={sortedData}
        columns={columns}
        keyExtractor={(item) => item.id}
        sortState={sortState}
        onSort={handleSort}
      />
    );
  },
};

export const WithSelection: Story = {
  render: () => {
    const [selectedRows, setSelectedRows] = useState<Set<string | number>>(new Set());

    const handleSelectRow = (key: string | number) => {
      setSelectedRows((prev) => {
        const next = new Set(prev);
        if (next.has(key)) next.delete(key);
        else next.add(key);
        return next;
      });
    };

    const handleSelectAll = () => {
      setSelectedRows((prev) => {
        const allSelected = sampleData.every((item) => prev.has(item.id));
        if (allSelected) return new Set();
        return new Set(sampleData.map((item) => item.id));
      });
    };

    return (
      <div>
        <div className="mb-4 text-sm text-gray-600">
          Selected: {selectedRows.size} items
        </div>
        <DataTable
          data={sampleData}
          columns={columns}
          keyExtractor={(item) => item.id}
          selectedRows={selectedRows}
          onSelectRow={handleSelectRow}
          onSelectAll={handleSelectAll}
        />
      </div>
    );
  },
};

export const ClickableRows: Story = {
  render: () => (
    <DataTable
      data={sampleData}
      columns={columns}
      keyExtractor={(item) => item.id}
      onRowClick={(item) => alert(`Clicked: ${item.name}`)}
    />
  ),
};

export const WithHook: Story = {
  name: 'Using useDataTable Hook',
  render: () => {
    const { sortState, handleSort, selectedRows, handleSelectRow, handleSelectAll, sortedData } =
      useDataTable(sampleData, {
        initialSort: { column: 'name', direction: 'asc' },
      });

    return (
      <div>
        <div className="mb-4 text-sm text-gray-600">
          Selected: {selectedRows.size} items
        </div>
        <DataTable
          data={sortedData}
          columns={columns}
          keyExtractor={(item) => item.id}
          sortState={sortState}
          onSort={handleSort}
          selectedRows={selectedRows}
          onSelectRow={handleSelectRow}
          onSelectAll={() => handleSelectAll(sampleData.map((p) => p.id))}
        />
      </div>
    );
  },
};
