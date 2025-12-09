import type { Meta, StoryObj } from '@storybook/react';
import { useState } from 'react';
import { Pagination, SimplePagination } from '../components/Pagination/Pagination';

const meta: Meta<typeof Pagination> = {
  title: 'Components/Pagination',
  component: Pagination,
  parameters: {
    layout: 'padded',
  },
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<typeof Pagination>;

export const Default: Story = {
  render: () => {
    const [currentPage, setCurrentPage] = useState(1);
    return (
      <Pagination
        currentPage={currentPage}
        totalPages={10}
        onPageChange={setCurrentPage}
        totalItems={100}
        itemsPerPage={10}
      />
    );
  },
};

export const FewPages: Story = {
  render: () => {
    const [currentPage, setCurrentPage] = useState(1);
    return (
      <Pagination
        currentPage={currentPage}
        totalPages={5}
        onPageChange={setCurrentPage}
        totalItems={50}
        itemsPerPage={10}
      />
    );
  },
};

export const ManyPages: Story = {
  render: () => {
    const [currentPage, setCurrentPage] = useState(15);
    return (
      <Pagination
        currentPage={currentPage}
        totalPages={50}
        onPageChange={setCurrentPage}
        totalItems={500}
        itemsPerPage={10}
      />
    );
  },
};

export const FirstPage: Story = {
  render: () => {
    const [currentPage, setCurrentPage] = useState(1);
    return (
      <Pagination
        currentPage={currentPage}
        totalPages={20}
        onPageChange={setCurrentPage}
        totalItems={200}
        itemsPerPage={10}
      />
    );
  },
};

export const LastPage: Story = {
  render: () => {
    const [currentPage, setCurrentPage] = useState(20);
    return (
      <Pagination
        currentPage={currentPage}
        totalPages={20}
        onPageChange={setCurrentPage}
        totalItems={200}
        itemsPerPage={10}
      />
    );
  },
};

export const WithoutItemCount: Story = {
  render: () => {
    const [currentPage, setCurrentPage] = useState(5);
    return (
      <Pagination
        currentPage={currentPage}
        totalPages={10}
        onPageChange={setCurrentPage}
        showItemCount={false}
      />
    );
  },
};

export const Simple: Story = {
  name: 'SimplePagination',
  render: () => {
    const [currentPage, setCurrentPage] = useState(3);
    return (
      <SimplePagination
        currentPage={currentPage}
        totalPages={10}
        onPageChange={setCurrentPage}
      />
    );
  },
};

export const SimpleAtStart: Story = {
  name: 'SimplePagination - First Page',
  render: () => {
    const [currentPage, setCurrentPage] = useState(1);
    return (
      <SimplePagination
        currentPage={currentPage}
        totalPages={10}
        onPageChange={setCurrentPage}
      />
    );
  },
};

export const SimpleAtEnd: Story = {
  name: 'SimplePagination - Last Page',
  render: () => {
    const [currentPage, setCurrentPage] = useState(10);
    return (
      <SimplePagination
        currentPage={currentPage}
        totalPages={10}
        onPageChange={setCurrentPage}
      />
    );
  },
};
