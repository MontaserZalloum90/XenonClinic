import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { BrowserRouter } from 'react-router-dom';
import { NotFound } from './NotFound';

// Wrap component with router for Link components
const renderWithRouter = (component: React.ReactNode) => {
  return render(<BrowserRouter>{component}</BrowserRouter>);
};

describe('NotFound Page', () => {
  it('renders 404 heading', () => {
    renderWithRouter(<NotFound />);
    expect(screen.getByText('404')).toBeInTheDocument();
  });

  it('renders Page Not Found message', () => {
    renderWithRouter(<NotFound />);
    expect(screen.getByText('Page Not Found')).toBeInTheDocument();
  });

  it('renders descriptive message', () => {
    renderWithRouter(<NotFound />);
    expect(
      screen.getByText(/Sorry, we couldn't find the page you're looking for/i)
    ).toBeInTheDocument();
  });

  it('renders Go to Dashboard link', () => {
    renderWithRouter(<NotFound />);
    const dashboardLink = screen.getByRole('link', { name: /Go to Dashboard/i });
    expect(dashboardLink).toBeInTheDocument();
    expect(dashboardLink).toHaveAttribute('href', '/');
  });

  it('renders Go Back button', () => {
    renderWithRouter(<NotFound />);
    expect(screen.getByRole('button', { name: /Go Back/i })).toBeInTheDocument();
  });

  it('calls window.history.back when Go Back is clicked', async () => {
    const user = userEvent.setup();
    const historyBackSpy = vi.spyOn(window.history, 'back').mockImplementation(() => {});

    renderWithRouter(<NotFound />);

    const goBackButton = screen.getByRole('button', { name: /Go Back/i });
    await user.click(goBackButton);

    expect(historyBackSpy).toHaveBeenCalledOnce();

    historyBackSpy.mockRestore();
  });

  it('renders support contact message', () => {
    renderWithRouter(<NotFound />);
    expect(
      screen.getByText(/If you believe this is an error, please contact support/i)
    ).toBeInTheDocument();
  });

  it('displays warning icon', () => {
    renderWithRouter(<NotFound />);
    // Check for the icon container with yellow background
    const iconContainer = document.querySelector('.bg-yellow-100');
    expect(iconContainer).toBeInTheDocument();
  });
});
