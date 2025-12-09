import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { StatusBadge } from './StatusBadge';
import { AppointmentStatus } from '../../types/appointment';

describe('StatusBadge', () => {
  it('renders Booked status correctly', () => {
    render(<StatusBadge status={AppointmentStatus.Booked} />);
    const badge = screen.getByText('Booked');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-blue-100', 'text-blue-800');
  });

  it('renders Confirmed status correctly', () => {
    render(<StatusBadge status={AppointmentStatus.Confirmed} />);
    const badge = screen.getByText('Confirmed');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-green-100', 'text-green-800');
  });

  it('renders CheckedIn status correctly', () => {
    render(<StatusBadge status={AppointmentStatus.CheckedIn} />);
    const badge = screen.getByText('Checked In');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-purple-100', 'text-purple-800');
  });

  it('renders Completed status correctly', () => {
    render(<StatusBadge status={AppointmentStatus.Completed} />);
    const badge = screen.getByText('Completed');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-gray-100', 'text-gray-800');
  });

  it('renders Cancelled status correctly', () => {
    render(<StatusBadge status={AppointmentStatus.Cancelled} />);
    const badge = screen.getByText('Cancelled');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-red-100', 'text-red-800');
  });

  it('renders NoShow status correctly', () => {
    render(<StatusBadge status={AppointmentStatus.NoShow} />);
    const badge = screen.getByText('No Show');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-yellow-100', 'text-yellow-800');
  });

  it('renders Unknown for invalid status', () => {
    render(<StatusBadge status={999 as AppointmentStatus} />);
    const badge = screen.getByText('Unknown');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-gray-100', 'text-gray-800');
  });

  it('applies correct base styling', () => {
    render(<StatusBadge status={AppointmentStatus.Booked} />);
    const badge = screen.getByText('Booked');
    expect(badge).toHaveClass(
      'inline-flex',
      'items-center',
      'px-2.5',
      'py-0.5',
      'rounded-full',
      'text-xs',
      'font-medium'
    );
  });
});
