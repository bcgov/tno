import { fireEvent, render, screen } from '@testing-library/react';
import { Formik } from 'formik';
import { ThemeProvider } from 'styled-components';

import {
  MinisterAliases,
  parseAliases,
  serializeAliases,
} from '@/editor/features/admin/ministers/MinisterAliases';

describe('minister aliases', () => {
  it('parses the stored comma-separated value', () => {
    expect(parseAliases(' Eby,Premier Eby , ,D. Eby')).toEqual(['Eby', 'Premier Eby', 'D. Eby']);
    expect(parseAliases(undefined)).toEqual([]);
  });

  it('serializes the rows, skipping empty ones', () => {
    expect(serializeAliases(['Eby', ' ', ' Premier Eby '])).toBe('Eby, Premier Eby');
  });

  it('adds, edits and removes alias rows', () => {
    let aliases = '';
    render(
      <ThemeProvider theme={{ css: {} }}>
        <Formik initialValues={{ aliases: 'Eby' }} onSubmit={() => {}}>
          {({ values }) => {
            aliases = values.aliases;
            return <MinisterAliases />;
          }}
        </Formik>
      </ThemeProvider>,
    );

    expect(screen.getByLabelText('Alias 1')).toHaveValue('Eby');

    fireEvent.click(screen.getByText('Add alias'));
    fireEvent.change(screen.getByLabelText('Alias 2'), { target: { value: 'Premier, Eby' } });
    expect(screen.getByLabelText('Alias 2')).toHaveValue('Premier Eby');
    expect(aliases).toBe('Eby, Premier Eby');

    fireEvent.click(screen.getByLabelText('Remove alias 1'));
    expect(screen.getByLabelText('Alias 1')).toHaveValue('Premier Eby');
    expect(aliases).toBe('Premier Eby');
  });
});
