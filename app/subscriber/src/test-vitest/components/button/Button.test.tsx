import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';

import { Button } from '../../../components/button';
import { TestWrapper } from '../../utils';

const renderWithWrapper = (component: React.ReactNode) => {
  return render(<TestWrapper>{component}</TestWrapper>);
};

// Test suite for the Button component
describe('Button Component', () => {
  // Verifies that the button correctly displays its text content
  it('should render the button text correctly', () => {
    renderWithWrapper(<Button>test button</Button>);
    // Check specified text exists
    expect(screen.getByText('test button')).toBeInTheDocument();
  });

  // Verifies handles click events
  it('should handle click events', () => {
    // Create a mock function to track click events
    const handleClick = vi.fn();
    renderWithWrapper(<Button onClick={handleClick}>click me</Button>);

    // Simulate a user clicking the button
    const button = screen.getByText('click me');
    fireEvent.click(button);
    // Verify that the click handler was called exactly once
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  // Verifies that disabled buttons do not trigger click events
  it('disabled button should not trigger click event', () => {
    // Create a mock function for the click handler
    const handleClick = vi.fn();
    renderWithWrapper(
      <Button disabled onClick={handleClick}>
        disabled button
      </Button>,
    );

    // Attempt to click the disabled button
    const button = screen.getByText('disabled button');
    fireEvent.click(button);
    // Verify that the click handler was not called
    expect(handleClick).not.toHaveBeenCalled();
  });

  //Verifies that the button type attribute can be customized
  it('should allow overriding button type', () => {
    renderWithWrapper(<Button type="submit">submit button</Button>);
    // Check if the button has the correct type attribute
    const button = screen.getByText('submit button');
    expect(button).toHaveAttribute('type', 'submit');
  });
});
