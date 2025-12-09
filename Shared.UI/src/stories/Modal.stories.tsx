import type { Meta, StoryObj } from '@storybook/react';
import { useState } from 'react';
import { Modal } from '../components/Modal/Modal';

const meta: Meta<typeof Modal> = {
  title: 'Components/Modal',
  component: Modal,
  parameters: {
    layout: 'centered',
  },
  tags: ['autodocs'],
  argTypes: {
    size: {
      control: 'select',
      options: ['sm', 'md', 'lg', 'xl'],
    },
  },
};

export default meta;
type Story = StoryObj<typeof Modal>;

const ModalWithTrigger = ({ size = 'md' }: { size?: 'sm' | 'md' | 'lg' | 'xl' }) => {
  const [isOpen, setIsOpen] = useState(false);
  return (
    <>
      <button
        onClick={() => setIsOpen(true)}
        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        Open Modal ({size})
      </button>
      <Modal
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        title="Modal Title"
        size={size}
      >
        <p className="text-gray-600 mb-4">
          This is a {size} modal. You can put any content here including forms,
          information, or confirmations.
        </p>
        <div className="flex justify-end gap-2">
          <button
            onClick={() => setIsOpen(false)}
            className="px-4 py-2 text-gray-700 border rounded-md hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            onClick={() => setIsOpen(false)}
            className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
          >
            Confirm
          </button>
        </div>
      </Modal>
    </>
  );
};

export const Small: Story = {
  render: () => <ModalWithTrigger size="sm" />,
};

export const Medium: Story = {
  render: () => <ModalWithTrigger size="md" />,
};

export const Large: Story = {
  render: () => <ModalWithTrigger size="lg" />,
};

export const ExtraLarge: Story = {
  render: () => <ModalWithTrigger size="xl" />,
};

const FormModal = () => {
  const [isOpen, setIsOpen] = useState(false);
  return (
    <>
      <button
        onClick={() => setIsOpen(true)}
        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
      >
        Open Form Modal
      </button>
      <Modal
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        title="Create New Item"
        size="md"
      >
        <form className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Name
            </label>
            <input
              type="text"
              className="w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-blue-500"
              placeholder="Enter name"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Description
            </label>
            <textarea
              className="w-full px-3 py-2 border rounded-md focus:ring-2 focus:ring-blue-500"
              rows={3}
              placeholder="Enter description"
            />
          </div>
          <div className="flex justify-end gap-2 pt-4">
            <button
              type="button"
              onClick={() => setIsOpen(false)}
              className="px-4 py-2 text-gray-700 border rounded-md hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
            >
              Create
            </button>
          </div>
        </form>
      </Modal>
    </>
  );
};

export const WithForm: Story = {
  render: () => <FormModal />,
};
