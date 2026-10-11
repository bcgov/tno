import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { Formik, useFormikContext } from 'formik';
import React from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { ReportEditContext } from '../../../features/my-reports/edit/ReportEditContext';
import { ReportSectionAI } from '../../../features/my-reports/edit/settings/template/ReportSectionAI';
import { TestWrapper } from '../../utils';

/** A tiny external store so the lookup can change while the section stays mounted - a remount
 *  would re-run the component's initializers and hide the very bug being tested. */
const lookup = vi.hoisted(() => {
  let llms: any[] = [];
  const listeners = new Set<() => void>();
  return {
    get: () => llms,
    set: (values: any[]) => {
      llms = values;
      listeners.forEach((listener) => listener());
    },
    subscribe: (listener: () => void) => {
      listeners.add(listener);
      return () => listeners.delete(listener);
    },
  };
});

/** The prompt tools' API, so the toolbar can be exercised without a server. */
const reportAI = vi.hoisted(() => ({
  describeSection: vi.fn(),
  describeInstanceSection: vi.fn(),
  testSection: vi.fn(),
  getTest: vi.fn(),
  chat: vi.fn(),
}));

vi.mock('store/hooks', async () => {
  const react = await import('react');
  return {
    useLookup: () => [
      { llms: react.useSyncExternalStore(lookup.subscribe, lookup.get) },
      { getLLMs: () => Promise.resolve(lookup.get()) },
    ],
    useReports: () => [{}, {}],
    useReportInstances: () => [{}, {}],
    useReportAI: () => reportAI,
    useApiHub: () => ({ useHubEffect: () => {} }),
  };
});
// Stable identity, the way a Redux selector behaves - a fresh object each render would re-run the
// section's effects every pass and mask a render loop.
const userInfo = vi.hoisted(() => ({ userInfo: { id: 1, roles: [] as string[] } }));

vi.mock('store/slices', () => ({
  useAppStore: () => [userInfo],
}));

const LLMS = [
  {
    id: 1,
    name: 'Alpha',
    isPublic: true,
    isEnabled: true,
    minTemperature: 0.1,
    userPrompt: 'alpha prompt',
  },
  {
    id: 2,
    name: 'Beta',
    isPublic: true,
    isEnabled: true,
    minTemperature: 0.5,
    userPrompt: 'beta prompt',
  },
];

const reportWith = (settings: Record<string, unknown>) =>
  ({
    id: 1,
    name: 'Report',
    sections: [{ name: 'ai', isEnabled: true, settings: { label: 'AI', ...settings } }],
  } as any);

/** Surfaces the live values so assertions can read what the section wrote. */
const Values = () => {
  const { values } = useFormikContext<any>();
  return <div data-testid="values">{JSON.stringify(values.sections[0].settings)}</div>;
};

const renderSection = (report: any) =>
  render(
    <TestWrapper>
      <Formik initialValues={report} onSubmit={() => {}}>
        {(formik) => (
          <ReportEditContext.Provider
            value={
              {
                values: formik.values,
                setFieldValue: formik.setFieldValue,
                setValues: formik.setValues,
              } as any
            }
          >
            <ReportSectionAI index={0} />
            <Values />
          </ReportEditContext.Provider>
        )}
      </Formik>
    </TestWrapper>,
  );

const settings = () => JSON.parse(screen.getByTestId('values').textContent ?? '{}');

describe('ReportSectionAI (subscriber)', () => {
  // jsdom has no modal dialogs; the open attribute stands in for the top layer.
  const originalShowModal = HTMLDialogElement.prototype.showModal;
  const originalCloseDialog = HTMLDialogElement.prototype.close;
  beforeEach(() => {
    lookup.set([...LLMS]);
    HTMLDialogElement.prototype.showModal = function () {
      this.open = true;
    };
    HTMLDialogElement.prototype.close = function () {
      this.open = false;
    };
  });
  afterEach(() => {
    HTMLDialogElement.prototype.showModal = originalShowModal;
    HTMLDialogElement.prototype.close = originalCloseDialog;
  });

  it('keeps the saved model, and its temperature and prompt', async () => {
    renderSection(
      reportWith({ llmId: 2, temperature: 0.9, userPrompt: 'the subscriber wrote this' }),
    );

    await waitFor(() => expect(screen.getByTestId('values')).toBeInTheDocument());
    expect(settings().llmId).toBe(2);
    expect(settings().temperature).toBe(0.9);
    // The Wysiwyg normalizes the value it is given, so match on the text rather than the markup.
    expect(settings().userPrompt).toContain('the subscriber wrote this');
  });

  it('does not lose the selected model when the lookup loads after the section renders', async () => {
    lookup.set([]);
    renderSection(reportWith({ llmId: 2, temperature: 0.9 }));
    await waitFor(() => expect(screen.getByTestId('values')).toBeInTheDocument());
    // Nothing to choose from yet, so the section must not write over the saved selection.
    expect(settings().llmId).toBe(2);

    // The lookup arrives while the section stays mounted.
    act(() => lookup.set([...LLMS]));

    await waitFor(() => expect(settings().llmId).toBe(2));
    expect(settings().temperature).toBe(0.9);
    // The field itself has to show it: the options used to be captured at mount, so a lookup that
    // arrived later left the control blank even though the value was still on the section.
    expect(screen.getByText('Beta')).toBeInTheDocument();
  });

  it('fills in the first available model when the section has none', async () => {
    renderSection(reportWith({}));
    await waitFor(() => expect(settings().llmId).toBe(1));
    expect(settings().temperature).toBe(0.1);
    expect(settings().userPrompt).toContain('alpha prompt');
  });
  it('always follows the prompt and shows saved input fields', async () => {
    renderSection(
      reportWith({ llmId: 1, aiOutputMode: 'TopicSummary', aiInputFields: ['headline'] }),
    );
    expect(screen.queryByText('Output format')).not.toBeInTheDocument();
    expect(screen.getByRole('checkbox', { name: 'Headline' })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Quotes' })).not.toBeChecked();
    fireEvent.click(screen.getByRole('checkbox', { name: 'Quotes' }));
    await waitFor(() => expect(settings().aiInputFields).toEqual(['headline', 'quotes']));
  });

  it('selects default fields for existing sections and can restore them', async () => {
    renderSection(reportWith({ llmId: 1 }));
    expect(screen.getByRole('checkbox', { name: 'Quotes' })).toBeChecked();
    expect(screen.getByRole('checkbox', { name: 'Article text' })).not.toBeChecked();
    fireEvent.click(screen.getByRole('checkbox', { name: 'Article text' }));
    await waitFor(() => expect(settings().aiInputFields).toContain('body'));
    fireEvent.click(screen.getByRole('checkbox', { name: 'Quotes' }));
    await waitFor(() => expect(settings().aiInputFields).not.toContain('quotes'));
    fireEvent.click(screen.getByRole('button', { name: 'Restore default fields' }));
    await waitFor(() => expect(settings().aiInputFields).toContain('quotes'));
    expect(settings().aiInputFields).not.toContain('body');
    expect(screen.getByRole('checkbox', { name: 'Article text' })).not.toBeChecked();
  });

  it('preserves an explicitly saved article text selection', () => {
    renderSection(reportWith({ llmId: 1, aiInputFields: ['summary', 'body'] }));
    expect(screen.getByRole('checkbox', { name: 'Article text' })).toBeChecked();
  });

  it('puts restore-default and schema help in the prompt editor toolbar', async () => {
    renderSection(reportWith({ llmId: 1, userPrompt: 'My prompt' }));
    const restore = screen.getByRole('button', { name: 'Use default user prompt' });
    expect(restore.closest('.toolbar')).not.toBeNull();
    expect(
      screen.getByRole('button', { name: 'Prompt data and story links' }).closest('.toolbar'),
    ).toBe(restore.closest('.toolbar'));
    expect(screen.getByRole('button', { name: 'Prompt data and story links' })).toHaveTextContent(
      'Prompt data',
    );
    // Replacing text the user wrote asks first; cancelling keeps it.
    fireEvent.click(restore);
    const confirm = screen.getByRole('dialog', { name: 'Replace the prompt?' });
    expect(confirm).toHaveAttribute('open');
    fireEvent.click(within(confirm).getByRole('button', { name: 'Cancel' }));
    expect(confirm).not.toHaveAttribute('open');
    expect(settings().userPrompt).toContain('My prompt');
    fireEvent.click(restore);
    fireEvent.click(within(confirm).getByRole('button', { name: 'Replace' }));
    await waitFor(() => expect(settings().userPrompt).toContain('alpha prompt'));
    expect(confirm).not.toHaveAttribute('open');
  });

  it('replaces an empty prompt with the default without asking', async () => {
    renderSection(reportWith({ llmId: 1, userPrompt: '<p><br></p>' }));
    fireEvent.click(screen.getByRole('button', { name: 'Use default user prompt' }));
    // A closed dialog is outside the accessibility tree, so look at the element itself.
    const confirm = document.querySelector('dialog[aria-label="Replace the prompt?"]');
    expect(confirm).not.toBeNull();
    expect(confirm).not.toHaveAttribute('open');
    await waitFor(() => expect(settings().userPrompt).toContain('alpha prompt'));
  });

  it('hides the restore action when the model has no default prompt', () => {
    lookup.set([{ ...LLMS[0], userPrompt: '<p><br></p>' }]);
    renderSection(reportWith({ llmId: 1, userPrompt: 'My prompt' }));
    expect(
      screen.queryByRole('button', { name: 'Use default user prompt' }),
    ).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Prompt data and story links' })).toBeInTheDocument();
  });
  it('opens schema help and restores the prompt while the editor is expanded', async () => {
    const originalShow = HTMLDialogElement.prototype.showModal;
    const originalClose = HTMLDialogElement.prototype.close;
    HTMLDialogElement.prototype.showModal = function () {
      this.open = true;
    };
    HTMLDialogElement.prototype.close = function () {
      this.open = false;
    };
    try {
      const { container } = renderSection(reportWith({ llmId: 1, userPrompt: 'My custom prompt' }));
      const toolbar = screen
        .getByRole('button', { name: 'Use default user prompt' })
        .closest('.toolbar')!;
      fireEvent.click(
        within(toolbar as HTMLElement).getByRole('button', { name: 'Popout editor' }),
      );
      const expanded = container.querySelector('#expand-modal') as HTMLDialogElement;
      await waitFor(() => expect(expanded.open).toBe(true));
      fireEvent.click(within(expanded).getByRole('button', { name: 'Use default user prompt' }));
      const confirm = screen
        .getAllByRole('dialog', { name: 'Replace the prompt?' })
        .find((d) => (d as HTMLDialogElement).open)!;
      fireEvent.click(within(confirm).getByRole('button', { name: 'Replace' }));
      await waitFor(() =>
        expect(expanded.querySelector('.ql-editor')).toHaveTextContent('alpha prompt'),
      );
      expect(settings().userPrompt).toContain('alpha prompt');
      fireEvent.click(
        within(expanded).getByRole('button', { name: 'Prompt data and story links' }),
      );
      const help = screen.getByRole('dialog', { name: 'Prompt data and story links' });
      expect(within(help).getByText('/view/:id')).toBeInTheDocument();
      expect(
        within(help).getByRole('heading', { name: 'Link data sent to the final writing step' }),
      ).toBeInTheDocument();
      expect(
        within(help).getByRole('heading', { name: 'Example prompt: view the story in a new tab' }),
      ).toBeInTheDocument();
      expect(help).toHaveTextContent('href="{url}"');
      expect(help).toHaveTextContent('href="{anchor}"');
      expect(
        within(help).getByRole('heading', { name: 'Example prompt: read within the report' }),
      ).toBeInTheDocument();
      expect(help).toHaveTextContent('Never construct or invent a URL');
      fireEvent.click(within(help).getByRole('button', { name: 'Close' }));
      expect(help).not.toHaveAttribute('open');
    } finally {
      HTMLDialogElement.prototype.showModal = originalShow;
      HTMLDialogElement.prototype.close = originalClose;
    }
  });

  it('offers the prompt tools in the toolbar only once the report is saved and generated', async () => {
    const originalShow = HTMLDialogElement.prototype.showModal;
    const originalClose = HTMLDialogElement.prototype.close;
    HTMLDialogElement.prototype.showModal = function () {
      this.open = true;
    };
    HTMLDialogElement.prototype.close = function () {
      this.open = false;
    };
    try {
      const { unmount } = renderSection(reportWith({ llmId: 1, userPrompt: 'My prompt' }));
      const view = screen.getByRole('button', { name: 'View prompt' });
      const workbench = screen.getByRole('button', { name: 'Prompt workbench' });
      expect(view).toBeDisabled();
      expect(workbench).toBeDisabled();
      expect(view).toHaveAttribute(
        'title',
        'Save the report to use the prompt tools for this section',
      );
      expect(view.closest('.toolbar')).toBe(
        screen.getByRole('button', { name: 'Prompt data and story links' }).closest('.toolbar'),
      );
      unmount();

      reportAI.describeSection.mockResolvedValue({
        sectionId: 9,
        sectionLabel: 'AI',
        llmName: 'Alpha',
        isPlanned: true,
        exchanges: [
          {
            stage: 'Map batch 1/1',
            kind: 'Map',
            sequence: 1,
            messages: [{ role: 'user', content: '[S1] Headline\nsection: news' }],
            durationMs: 0,
            jsonMode: true,
            isTruncated: false,
            estimatedTokens: 12,
          },
        ],
      });
      renderSection({
        id: 1,
        name: 'Report',
        instances: [{ id: 7 }],
        sections: [
          {
            id: 9,
            name: 'ai',
            isEnabled: true,
            settings: { label: 'AI', llmId: 1, userPrompt: 'My prompt' },
          },
        ],
      } as any);
      const enabledView = screen.getByRole('button', { name: 'View prompt' });
      expect(enabledView).toBeEnabled();
      expect(screen.getByRole('button', { name: 'Prompt workbench' })).toBeEnabled();
      fireEvent.click(enabledView);
      await waitFor(() =>
        expect(reportAI.describeSection).toHaveBeenCalledWith(
          1,
          9,
          expect.objectContaining({ label: 'AI', llmId: 1 }),
        ),
      );
      const dialog = await screen.findByRole('dialog', { name: 'AI section prompt: AI' });
      expect(within(dialog).getByText('Map batch 1/1')).toBeInTheDocument();
      expect(dialog).toHaveTextContent('Planned prompt');
      expect(dialog).toHaveTextContent('section: news');
    } finally {
      HTMLDialogElement.prototype.showModal = originalShow;
      HTMLDialogElement.prototype.close = originalClose;
    }
  });
});
