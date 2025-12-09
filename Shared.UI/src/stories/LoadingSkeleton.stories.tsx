import type { Meta, StoryObj } from '@storybook/react';
import { LoadingSkeleton, SkeletonText, SkeletonCard, SkeletonTable } from '../components/LoadingSkeleton/LoadingSkeleton';

const meta: Meta<typeof LoadingSkeleton> = {
  title: 'Components/LoadingSkeleton',
  component: LoadingSkeleton,
  parameters: {
    layout: 'padded',
  },
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: 'select',
      options: ['text', 'rectangular', 'circular'],
    },
  },
};

export default meta;
type Story = StoryObj<typeof LoadingSkeleton>;

export const Text: Story = {
  args: {
    variant: 'text',
    width: '200px',
  },
};

export const Rectangular: Story = {
  args: {
    variant: 'rectangular',
    width: '300px',
    height: '150px',
  },
};

export const Circular: Story = {
  args: {
    variant: 'circular',
    width: '60px',
    height: '60px',
  },
};

export const TextBlock: Story = {
  name: 'SkeletonText',
  render: () => (
    <div className="w-80">
      <SkeletonText lines={4} />
    </div>
  ),
};

export const Card: Story = {
  name: 'SkeletonCard',
  render: () => (
    <div className="w-80">
      <SkeletonCard />
    </div>
  ),
};

export const Table: Story = {
  name: 'SkeletonTable',
  render: () => (
    <div className="w-full max-w-2xl">
      <SkeletonTable rows={4} columns={5} />
    </div>
  ),
};

export const ProfileCard: Story = {
  name: 'Profile Card Pattern',
  render: () => (
    <div className="flex items-center gap-4 p-4 border rounded-lg w-80">
      <LoadingSkeleton variant="circular" width="48px" height="48px" />
      <div className="flex-1">
        <LoadingSkeleton variant="text" width="120px" className="mb-2" />
        <LoadingSkeleton variant="text" width="80px" />
      </div>
    </div>
  ),
};
