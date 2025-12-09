import type { Meta, StoryObj } from '@storybook/react';
import { ToastProvider, useToast } from '../components/Toast/Toast';

const meta: Meta = {
  title: 'Components/Toast',
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  decorators: [
    (Story) => (
      <ToastProvider>
        <Story />
      </ToastProvider>
    ),
  ],
};

export default meta;
type Story = StoryObj;

const ToastDemo = () => {
  const { showToast } = useToast();

  return (
    <div className="flex flex-wrap gap-2">
      <button
        onClick={() => showToast('success', 'Operation completed successfully!')}
        className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700"
      >
        Success Toast
      </button>
      <button
        onClick={() => showToast('error', 'Something went wrong. Please try again.')}
        className="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700"
      >
        Error Toast
      </button>
      <button
        onClick={() => showToast('warning', 'Please review your input before continuing.')}
        className="px-4 py-2 bg-yellow-600 text-white rounded-md hover:bg-yellow-700"
      >
        Warning Toast
      </button>
      <button
        onClick={() => showToast('info', 'New updates are available for your account.')}
        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        Info Toast
      </button>
    </div>
  );
};

export const AllTypes: Story = {
  render: () => <ToastDemo />,
};

const CustomDurationDemo = () => {
  const { showToast } = useToast();

  return (
    <div className="flex flex-wrap gap-2">
      <button
        onClick={() => showToast('info', 'This toast will stay for 2 seconds', 2000)}
        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        2 Second Toast
      </button>
      <button
        onClick={() => showToast('info', 'This toast will stay for 10 seconds', 10000)}
        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        10 Second Toast
      </button>
    </div>
  );
};

export const CustomDuration: Story = {
  render: () => <CustomDurationDemo />,
};

const MultipleToastsDemo = () => {
  const { showToast } = useToast();

  const showMultiple = () => {
    showToast('success', 'First toast - Success!');
    setTimeout(() => showToast('info', 'Second toast - Info!'), 300);
    setTimeout(() => showToast('warning', 'Third toast - Warning!'), 600);
    setTimeout(() => showToast('error', 'Fourth toast - Error!'), 900);
  };

  return (
    <button
      onClick={showMultiple}
      className="px-4 py-2 bg-purple-600 text-white rounded-md hover:bg-purple-700"
    >
      Show Multiple Toasts
    </button>
  );
};

export const MultipleToasts: Story = {
  render: () => <MultipleToastsDemo />,
};
