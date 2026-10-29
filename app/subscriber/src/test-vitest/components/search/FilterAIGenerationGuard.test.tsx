import { act, fireEvent, render, screen } from '@testing-library/react';
import { filterFormat } from 'features/search-page/utils/filterFormat';
import React from 'react';
import { IFilterSettingsModel, useFilterAIGenerationGuard } from 'tno-core';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { TestWrapper } from '../../utils';

const generate = vi.fn();
const proceed = vi.fn();
const original: IFilterSettingsModel = {
  search: 'old criteria',
  searchUnpublished: false,
  size: 10,
};
const generated = {
  settings: { ...original, search: 'wildfire' },
  explanation: 'Wildfire stories',
  steps: [],
};

const Harness = ({ initialPrompt = '', initialSettings = original }) => {
  const [prompt, setPrompt] = React.useState(initialPrompt);
  const [settings, setSettings] = React.useState(initialSettings);
  const { confirmAction, confirmation } = useFilterAIGenerationGuard(
    prompt,
    settings,
    generate,
    (result) => setSettings(result.settings),
  );
  return (
    <>
      <input aria-label="Prompt" value={prompt} onChange={(e) => setPrompt(e.target.value)} />
      <button onClick={() => confirmAction('search', proceed)}>Search</button>
      <button onClick={() => confirmAction('save', proceed)}>Save</button>
      {confirmation}
    </>
  );
};
const click = (name: string) => fireEvent.click(screen.getByRole('button', { name, hidden: true }));
const mount = (prompt = '', settings = original) =>
  render(
    <TestWrapper>
      <Harness initialPrompt={prompt} initialSettings={settings} />
    </TestWrapper>,
  );

describe('Prompt generation before Search or Save', () => {
  beforeEach(() => {
    generate.mockReset().mockResolvedValue(generated);
    proceed.mockReset();
  });

  it.each(['', '<p><br></p>', '   '])('does not interrupt an empty prompt %s', (prompt) => {
    mount(prompt);
    click('Search');
    expect(proceed).toHaveBeenCalledWith(original);
    expect(screen.queryByText(/Generate the filter before/)).not.toBeInTheDocument();
  });

  it.each(['search', 'save'])(
    'generates and passes fresh settings to %s, then records the prompt',
    async (action) => {
      mount('Wildfires today');
      click(action === 'save' ? 'Save' : 'Search');
      expect(proceed).not.toHaveBeenCalled();
      click(`Generate and ${action}`);
      await act(async () => {});
      expect(generate).toHaveBeenCalledWith({ prompt: 'Wildfires today', settings: original });
      expect(proceed).toHaveBeenCalledWith({
        ...generated.settings,
        generatedPrompt: 'Wildfires today',
      });
      click('Search');
      expect(proceed).toHaveBeenCalledTimes(2);
      expect(generate).toHaveBeenCalledTimes(1);
    },
  );

  it('asks again after the prompt changes, and not after it is reverted', () => {
    mount('Wildfires today', { ...original, generatedPrompt: 'Wildfires today' });
    fireEvent.change(screen.getByLabelText('Prompt'), { target: { value: 'Floods today' } });
    click('Save');
    expect(screen.getByText('Generate the filter before saving?')).toBeInTheDocument();
    click('Cancel');
    expect(proceed).not.toHaveBeenCalled();
    fireEvent.change(screen.getByLabelText('Prompt'), { target: { value: 'Wildfires today' } });
    click('Search');
    expect(proceed).toHaveBeenCalledTimes(1);
  });

  it('can save without generation without marking the prompt as generated', () => {
    mount('New prompt');
    click('Save');
    click('Save without generating');
    expect(proceed).toHaveBeenCalledWith(original);
    expect(generate).not.toHaveBeenCalled();
    click('Search');
    expect(screen.getByText('Generate the filter before searching?')).toBeInTheDocument();
  });

  it('keeps the confirmation open when generation fails and allows retry', async () => {
    generate.mockRejectedValueOnce(new Error('Generation failed'));
    mount('New prompt');
    click('Search');
    click('Generate and search');
    await act(async () => {});
    expect(screen.getByRole('alert', { hidden: true })).toHaveTextContent('Generation failed');
    expect(proceed).not.toHaveBeenCalled();
    click('Generate and search');
    await act(async () => {});
    expect(proceed).toHaveBeenCalledTimes(1);
  });

  it('preserves generation history through saved settings formatting and HTML display changes', () => {
    const settings = filterFormat({ ...original, generatedPrompt: '<p>Wildfires today</p>' });
    mount('Wildfires today', settings);
    click('Search');
    expect(proceed).toHaveBeenCalledWith(settings);
    expect(generate).not.toHaveBeenCalled();
  });
});
