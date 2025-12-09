import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent, act } from '@testing-library/react';
import { ToastProvider, useToast } from '../components/Toast/Toast';

// Helper component to trigger toast
const ToastTrigger = ({ type, message }: { type: 'success' | 'error' | 'info' | 'warning'; message: string }) => {
  const { showToast } = useToast();
  return (
    <button onClick={() => showToast(type, message)} data-testid="trigger">
      Show Toast
    </button>
  );
};

describe('Toast', () => {
  it('renders ToastProvider without crashing', () => {
    render(
      <ToastProvider>
        <div>Test</div>
      </ToastProvider>
    );
    expect(screen.getByText('Test')).toBeInTheDocument();
  });

  it('shows toast when triggered', async () => {
    render(
      <ToastProvider>
        <ToastTrigger type="success" message="Success message" />
      </ToastProvider>
    );

    fireEvent.click(screen.getByTestId('trigger'));
    expect(screen.getByText('Success message')).toBeInTheDocument();
  });

  it('shows different toast types', async () => {
    const { rerender } = render(
      <ToastProvider>
        <ToastTrigger type="error" message="Error message" />
      </ToastProvider>
    );

    fireEvent.click(screen.getByTestId('trigger'));
    expect(screen.getByText('Error message')).toBeInTheDocument();
  });

  it('throws error when useToast is used outside provider', () => {
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => {});

    expect(() => {
      render(<ToastTrigger type="info" message="test" />);
    }).toThrow('useToast must be used within ToastProvider');

    consoleError.mockRestore();
  });
});
