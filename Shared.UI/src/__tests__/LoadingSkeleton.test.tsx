import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { LoadingSkeleton, SkeletonText, SkeletonCard, SkeletonTable } from '../components/LoadingSkeleton/LoadingSkeleton';

describe('LoadingSkeleton', () => {
  it('renders with default props', () => {
    render(<LoadingSkeleton />);
    expect(screen.getByRole('status')).toBeInTheDocument();
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('renders with text variant by default', () => {
    const { container } = render(<LoadingSkeleton />);
    expect(container.firstChild).toHaveClass('rounded', 'h-4');
  });

  it('renders with rectangular variant', () => {
    const { container } = render(<LoadingSkeleton variant="rectangular" />);
    expect(container.firstChild).toHaveClass('rounded-lg');
  });

  it('renders with circular variant', () => {
    const { container } = render(<LoadingSkeleton variant="circular" />);
    expect(container.firstChild).toHaveClass('rounded-full');
  });

  it('applies custom width and height', () => {
    const { container } = render(<LoadingSkeleton width="200px" height="100px" />);
    expect(container.firstChild).toHaveStyle({ width: '200px', height: '100px' });
  });

  it('applies custom className', () => {
    const { container } = render(<LoadingSkeleton className="custom-class" />);
    expect(container.firstChild).toHaveClass('custom-class');
  });
});

describe('SkeletonText', () => {
  it('renders with default 3 lines', () => {
    const { container } = render(<SkeletonText />);
    const skeletons = container.querySelectorAll('[role="status"]');
    expect(skeletons).toHaveLength(3);
  });

  it('renders with custom number of lines', () => {
    const { container } = render(<SkeletonText lines={5} />);
    const skeletons = container.querySelectorAll('[role="status"]');
    expect(skeletons).toHaveLength(5);
  });
});

describe('SkeletonCard', () => {
  it('renders card skeleton', () => {
    const { container } = render(<SkeletonCard />);
    const skeletons = container.querySelectorAll('[role="status"]');
    expect(skeletons.length).toBeGreaterThan(0);
  });

  it('applies custom className', () => {
    const { container } = render(<SkeletonCard className="custom-card" />);
    expect(container.firstChild).toHaveClass('custom-card');
  });
});

describe('SkeletonTable', () => {
  it('renders table skeleton with default rows and columns', () => {
    const { container } = render(<SkeletonTable />);
    const skeletons = container.querySelectorAll('[role="status"]');
    // 6 columns in header + 5 rows * 6 columns = 36
    expect(skeletons).toHaveLength(36);
  });

  it('renders with custom rows and columns', () => {
    const { container } = render(<SkeletonTable rows={3} columns={4} />);
    const skeletons = container.querySelectorAll('[role="status"]');
    // 4 columns in header + 3 rows * 4 columns = 16
    expect(skeletons).toHaveLength(16);
  });
});
