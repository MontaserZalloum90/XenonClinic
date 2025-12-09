import type { Meta, StoryObj } from '@storybook/react';
import { EmptyState } from '../components/EmptyState/EmptyState';

const meta: Meta<typeof EmptyState> = {
  title: 'Components/EmptyState',
  component: EmptyState,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof EmptyState>;

const SearchIcon = () => (
  <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
  </svg>
);

const DocumentIcon = () => (
  <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
  </svg>
);

const InboxIcon = () => (
  <svg className="w-8 h-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
  </svg>
);

export const Default: Story = {
  args: {
    title: 'No items found',
  },
};

export const WithDescription: Story = {
  args: {
    title: 'No results found',
    description: 'Try adjusting your search or filter to find what you are looking for.',
  },
};

export const WithIcon: Story = {
  args: {
    icon: <SearchIcon />,
    title: 'No search results',
    description: 'We could not find any matches for your search.',
  },
};

export const WithAction: Story = {
  args: {
    icon: <DocumentIcon />,
    title: 'No documents',
    description: 'Get started by creating your first document.',
    action: {
      label: 'Create Document',
      onClick: () => alert('Create clicked!'),
    },
  },
};

export const NoMessages: Story = {
  args: {
    icon: <InboxIcon />,
    title: 'Your inbox is empty',
    description: 'Messages from your team will appear here.',
  },
};

export const FullExample: Story = {
  args: {
    icon: <SearchIcon />,
    title: 'No patients found',
    description: 'There are no patients matching your current filters. Try adjusting your search criteria or add a new patient.',
    action: {
      label: 'Add New Patient',
      onClick: () => alert('Add patient clicked!'),
    },
  },
};
