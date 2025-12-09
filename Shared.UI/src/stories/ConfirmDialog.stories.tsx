import type { Meta, StoryObj } from '@storybook/react';
import { useState } from 'react';
import { ConfirmDialog, useConfirmDialog } from '../components/ConfirmDialog/ConfirmDialog';

const meta: Meta<typeof ConfirmDialog> = {
  title: 'Components/ConfirmDialog',
  component: ConfirmDialog,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    variant: {
      control: 'select',
      options: ['danger', 'warning', 'info'],
    },
  },
};

export default meta;
type Story = StoryObj<typeof ConfirmDialog>;

const ConfirmDialogDemo = ({
  variant = 'danger',
  title = 'Confirm Action',
  message = 'Are you sure you want to proceed?',
}: {
  variant?: 'danger' | 'warning' | 'info';
  title?: string;
  message?: string;
}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const handleConfirm = async () => {
    setIsLoading(true);
    await new Promise((resolve) => setTimeout(resolve, 1500));
    setIsLoading(false);
    setIsOpen(false);
    alert('Action confirmed!');
  };

  return (
    <>
      <button
        onClick={() => setIsOpen(true)}
        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        Open {variant} dialog
      </button>
      <ConfirmDialog
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        onConfirm={handleConfirm}
        title={title}
        message={message}
        variant={variant}
        isLoading={isLoading}
      />
    </>
  );
};

export const Danger: Story = {
  render: () => (
    <ConfirmDialogDemo
      variant="danger"
      title="Delete Item"
      message="Are you sure you want to delete this item? This action cannot be undone."
    />
  ),
};

export const Warning: Story = {
  render: () => (
    <ConfirmDialogDemo
      variant="warning"
      title="Unsaved Changes"
      message="You have unsaved changes. Are you sure you want to leave this page?"
    />
  ),
};

export const Info: Story = {
  render: () => (
    <ConfirmDialogDemo
      variant="info"
      title="Confirm Submission"
      message="You are about to submit this form. Would you like to proceed?"
    />
  ),
};

const UseConfirmDialogDemo = () => {
  const { dialogState, showConfirm, hideConfirm, handleConfirm } = useConfirmDialog();

  return (
    <div className="space-y-2">
      <button
        onClick={() =>
          showConfirm(
            'Delete User',
            'Are you sure you want to delete this user? All their data will be permanently removed.',
            () => alert('User deleted!'),
            'danger'
          )
        }
        className="block w-full px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
      >
        Delete User
      </button>
      <button
        onClick={() =>
          showConfirm(
            'Archive Project',
            'This project will be archived and hidden from the main view.',
            () => alert('Project archived!'),
            'warning'
          )
        }
        className="block w-full px-4 py-2 bg-yellow-600 text-white rounded-md hover:bg-yellow-700"
      >
        Archive Project
      </button>
      <button
        onClick={() =>
          showConfirm(
            'Send Notification',
            'This will send a notification to all team members.',
            () => alert('Notification sent!'),
            'info'
          )
        }
        className="block w-full px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        Send Notification
      </button>
      <ConfirmDialog
        isOpen={dialogState.isOpen}
        onClose={hideConfirm}
        onConfirm={handleConfirm}
        title={dialogState.title}
        message={dialogState.message}
        variant={dialogState.variant}
      />
    </div>
  );
};

export const WithHook: Story = {
  name: 'Using useConfirmDialog Hook',
  render: () => <UseConfirmDialogDemo />,
};
