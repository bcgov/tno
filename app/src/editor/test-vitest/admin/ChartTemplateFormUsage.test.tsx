import { act, render, screen } from '@testing-library/react';
import { Provider } from 'react-redux';
import { MemoryRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';
import { vi } from 'vitest';

import { store } from '@/editor/store';

const mocks = vi.hoisted(() => ({ get: vi.fn() }));

vi.mock('tno-core', async (importOriginal) => ({
  ...(await importOriginal<typeof import('tno-core')>()),
  useApi: () => ({ get: mocks.get }),
}));

const renderUsage = async () => {
  // Loaded after the store, which it reaches through a circular import.
  const { ChartTemplateFormUsage } =
    await import('@/editor/features/admin/charts/ChartTemplateFormUsage');
  render(
    <Provider store={store}>
      <ThemeProvider theme={{ css: {} }}>
        <MemoryRouter>
          <ChartTemplateFormUsage chartTemplateId={4} />
        </MemoryRouter>
      </ThemeProvider>
    </Provider>,
  );
};

describe('ChartTemplateFormUsage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('requests once and lists the reports', async () => {
    mocks.get.mockResolvedValue({
      data: [{ id: 5, name: 'A report', sections: ['One', 'Two'], owner: 'me', isEnabled: true }],
    });

    await renderUsage();

    expect(await screen.findByText('A report')).toBeInTheDocument();
    expect(screen.getByText('One, Two')).toBeInTheDocument();
    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 100));
    });
    expect(mocks.get).toHaveBeenCalledTimes(1);
    expect(mocks.get).toHaveBeenCalledWith('/admin/chart/templates/4/usage');
  });

  it('says when no report uses the template', async () => {
    mocks.get.mockResolvedValue({ data: [] });

    await renderUsage();

    expect(await screen.findByText('No reports use this chart template.')).toBeInTheDocument();
  });
});
