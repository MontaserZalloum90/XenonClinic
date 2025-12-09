import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { EmptyState } from '../components/EmptyState/EmptyState';

describe('EmptyState', () => {
  it('renders with title', () => {
    render(<EmptyState title="No items found" />);
    expect(screen.getByText('No items found')).toBeInTheDocument();
  });

  it('renders with description', () => {
    render(
      <EmptyState
        title="No items"
        description="There are no items to display"
      />
    );
    expect(screen.getByText('There are no items to display')).toBeInTheDocument();
  });

  it('renders with icon', () => {
    render(
      <EmptyState
        title="No items"
        icon={<span data-testid="custom-icon">Icon</span>}
      />
    );
    expect(screen.getByTestId('custom-icon')).toBeInTheDocument();
  });

  it('renders with action button', () => {
    const handleClick = vi.fn();
    render(
      <EmptyState
        title="No items"
        action={{ label: 'Add Item', onClick: handleClick }}
      />
    );

    const button = screen.getByText('Add Item');
    expect(button).toBeInTheDocument();

    fireEvent.click(button);
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('applies custom className', () => {
    const { container } = render(
      <EmptyState title="No items" className="custom-class" />
    );
    expect(container.firstChild).toHaveClass('custom-class');
  });

  it('does not render description when not provided', () => {
    render(<EmptyState title="No items" />);
    const description = screen.queryByText(/description/i);
    expect(description).not.toBeInTheDocument();
  });

  it('does not render action when not provided', () => {
    render(<EmptyState title="No items" />);
    const button = screen.queryByRole('button');
    expect(button).not.toBeInTheDocument();
  });
});
