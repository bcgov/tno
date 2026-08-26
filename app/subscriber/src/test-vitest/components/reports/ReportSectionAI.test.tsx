import { act, render, screen, waitFor } from '@testing-library/react';
import { Formik, useFormikContext } from 'formik';
import React from 'react';
import { describe, expect, it, vi } from 'vitest';

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

vi.mock('store/hooks', async () => {
  const react = await import('react');
  return {
    useLookup: () => [
      { llms: react.useSyncExternalStore(lookup.subscribe, lookup.get) },
      { getLLMs: () => Promise.resolve(lookup.get()) },
    ],
    useReports: () => [{}, {}],
    useReportInstances: () => [{}, {}],
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
  beforeEach(() => {
    lookup.set([...LLMS]);
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
});
