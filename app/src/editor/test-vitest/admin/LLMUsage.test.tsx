import { act, fireEvent, render, screen, waitFor } from '@testing-library/react';
import { Provider } from 'react-redux';
import { MemoryRouter } from 'react-router-dom';
import { ThemeProvider } from 'styled-components';
import { vi } from 'vitest';

import { store } from '@/editor/store';

const mocks = vi.hoisted(() => ({
  get: vi.fn(),
  findAllLLMs: vi.fn(),
}));

vi.mock('@/shared', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@/shared')>()),
  useApi: () => ({ get: mocks.get }),
  useApiAdminLLMs: () => ({ findAllLLMs: mocks.findAllLLMs }),
}));

describe('LLMUsage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('requests usage and connections once', async () => {
    // Loaded after the store, which it reaches through a circular import.
    const { LLMUsage } = await import('@/editor/features/admin/llms/LLMUsage');

    mocks.get.mockResolvedValue({
      data: { items: [], contentAnalysisResultCount: 0 },
    });
    // A new array each time, as a real response is, so each one changes the admin store.
    mocks.findAllLLMs.mockImplementation(async () => ({
      data: [{ id: 1, name: 'one', isEnabled: true }],
    }));

    render(
      <Provider store={store}>
        <ThemeProvider theme={{ css: {} }}>
          <MemoryRouter>
            <LLMUsage llmId={1} />
          </MemoryRouter>
        </ThemeProvider>
      </Provider>,
    );

    await waitFor(() => expect(mocks.findAllLLMs).toHaveBeenCalled());
    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 200));
    });

    expect(mocks.findAllLLMs).toHaveBeenCalledTimes(1);
    expect(mocks.get).toHaveBeenCalledTimes(1);
    expect(mocks.get).toHaveBeenCalledWith('/admin/llms/1/usage');
  });

  it('shows the migration details in the confirmation', async () => {
    const { LLMUsage } = await import('@/editor/features/admin/llms/LLMUsage');
    mocks.get.mockImplementation(async (url: string) => ({
      data: url.endsWith('/usage')
        ? {
            items: [{ type: 'ReportSection', id: 5, sectionId: 6, name: 'A report' }],
            contentAnalysisResultCount: 0,
          }
        : { errors: [], warnings: ['A warning'] },
    }));
    mocks.findAllLLMs.mockResolvedValue({
      data: [
        { id: 1, name: 'one', isEnabled: true },
        { id: 2, name: 'two', isEnabled: true },
      ],
    });

    render(
      <Provider store={store}>
        <ThemeProvider theme={{ css: {} }}>
          <MemoryRouter>
            <LLMUsage llmId={1} />
          </MemoryRouter>
        </ThemeProvider>
      </Provider>,
    );

    const button = await screen.findByRole('button', { name: 'Migrate 1 item(s)' });
    fireEvent.keyDown(screen.getByRole('combobox'), { key: 'ArrowDown' });
    fireEvent.click(await screen.findByText('two'));
    await waitFor(() => expect(button).toBeEnabled());
    fireEvent.click(button);

    expect(await screen.findByText(/Move 1 item\(s\) to two\?/)).toBeInTheDocument();
    expect(document.body).toHaveTextContent('A warning');
    expect(document.body).not.toHaveTextContent('[object Object]');
  });
});
