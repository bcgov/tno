import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ComboBox } from 'features/admin/automation/designer/ComboBox';
import React from 'react';
import { TestWrapper } from 'test/utils';

it('allows choosing, clearing, and entering a freeform section with an associated label', async () => {
  const user = userEvent.setup();
  const Field = () => {
    const [value, setValue] = React.useState('Front');
    return (
      <>
        <ComboBox
          name="section"
          label="Section"
          value={value}
          suggestions={['Front', 'Sports']}
          isClearable
          onChange={setValue}
        />
        <output data-testid="value">{value}</output>
      </>
    );
  };
  const { container } = render(
    <TestWrapper>
      <Field />
    </TestWrapper>,
  );
  const input = screen.getByLabelText('Section');
  await user.click(container.querySelector('.rs__clear-indicator')!);
  expect(screen.getByTestId('value')).toBeEmptyDOMElement();
  await user.click(input);
  await user.click(screen.getByRole('option', { name: 'Sports' }));
  expect(screen.getByTestId('value')).toHaveTextContent('Sports');
  await user.type(input, 'Local news');
  await user.tab();
  expect(screen.getByTestId('value')).toHaveTextContent('Local news');
});
