import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { Forbidden } from './Forbidden';
import { AuthProvider } from '../../contexts/AuthContext';

// Mock the auth API
vi.mock('../../lib/api', () => ({
  authApi: {
    getCurrentUser: vi.fn().mockRejectedValue(new Error('Not authenticated')),
  },
}));

// Wrap component with required providers
const renderWithProviders = (component: React.ReactNode) => {
  return render(
    <BrowserRouter>
      <AuthProvider>{component}</AuthProvider>
    </BrowserRouter>
  );
};

describe('Forbidden Page', () => {
  it('renders 403 heading', () => {
    renderWithProviders(<Forbidden />);
    expect(screen.getByText('403')).toBeInTheDocument();
  });

  it('renders Access Forbidden message', () => {
    renderWithProviders(<Forbidden />);
    expect(screen.getByText('Access Forbidden')).toBeInTheDocument();
  });

  it('renders descriptive message about permission', () => {
    renderWithProviders(<Forbidden />);
    expect(
      screen.getByText(/Sorry, you don't have permission to access this page/i)
    ).toBeInTheDocument();
  });

  it('renders Go to Dashboard link', () => {
    renderWithProviders(<Forbidden />);
    const dashboardLink = screen.getByRole('link', { name: /Go to Dashboard/i });
    expect(dashboardLink).toBeInTheDocument();
    expect(dashboardLink).toHaveAttribute('href', '/');
  });

  it('renders Go Back button', () => {
    renderWithProviders(<Forbidden />);
    expect(screen.getByRole('button', { name: /Go Back/i })).toBeInTheDocument();
  });

  it('calls window.history.back when Go Back is clicked', async () => {
    const user = userEvent.setup();
    const historyBackSpy = vi.spyOn(window.history, 'back').mockImplementation(() => {});

    renderWithProviders(<Forbidden />);

    const goBackButton = screen.getByRole('button', { name: /Go Back/i });
    await user.click(goBackButton);

    expect(historyBackSpy).toHaveBeenCalledOnce();

    historyBackSpy.mockRestore();
  });

  it('renders administrator contact message', () => {
    renderWithProviders(<Forbidden />);
    expect(
      screen.getByText(/If you need access to this area, please contact your administrator/i)
    ).toBeInTheDocument();
  });

  it('displays shield icon', () => {
    renderWithProviders(<Forbidden />);
    // Check for the icon container with red background
    const iconContainer = document.querySelector('.bg-red-100');
    expect(iconContainer).toBeInTheDocument();
  });
});
