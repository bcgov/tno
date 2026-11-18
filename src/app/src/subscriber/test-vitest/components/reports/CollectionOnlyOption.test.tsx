import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Formik } from 'formik';
import { describe, expect, it } from 'vitest';

import { CollectionOnlyOption } from '../../../features/my-reports/edit/settings/CollectionOnlyOption';
import { TestWrapper } from '../../utils/TestWrapper';

const renderOption = (collectionOnly?: boolean) => {
  const values = { settings: { collectionOnly } };
  const view = render(
    <TestWrapper>
      <Formik initialValues={values} onSubmit={() => {}}>
        {({ values: current }) => (
          <>
            <CollectionOnlyOption />
            <output data-testid="value">{String(!!current.settings.collectionOnly)}</output>
          </>
        )}
      </Formik>
    </TestWrapper>,
  );
  return { ...view, checkbox: () => screen.getByLabelText(/^Collection only/) };
};

describe('Collection only report option', () => {
  it('treats a report without the setting as not collection only', () => {
    const { checkbox } = renderOption(undefined);
    expect(checkbox()).not.toBeChecked();
    expect(screen.getByTestId('value')).toHaveTextContent('false');
  });

  it('turns the setting on and off', async () => {
    const { checkbox } = renderOption(false);
    await userEvent.click(checkbox());
    expect(checkbox()).toBeChecked();
    expect(screen.getByTestId('value')).toHaveTextContent('true');
    await userEvent.click(checkbox());
    expect(screen.getByTestId('value')).toHaveTextContent('false');
  });

  it('explains that the report is never sent and previews headlines only', () => {
    renderOption(true);
    expect(
      screen.getByText(/never emailed, and its preview lists headlines only/),
    ).toBeInTheDocument();
  });
});
