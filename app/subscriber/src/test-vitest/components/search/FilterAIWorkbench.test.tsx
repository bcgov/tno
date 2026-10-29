import { act, fireEvent, render, screen, within } from '@testing-library/react';
import React from 'react';
import { FilterAIWorkbench, IFilterAIManualFilterProps, IFilterSettingsModel } from 'tno-core';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { TestWrapper } from '../../utils';

const stories = {
  hits: {
    total: { value: 2, relation: 'eq' },
    hits: [
      {
        _source: {
          id: 1,
          headline: 'Fire in town',
          publishedOn: '2026-10-21T10:00:00Z',
          source: { code: 'CBC', name: 'CBC News' },
          mediaType: { name: 'Online' },
          tonePools: [{ value: 1 }],
        },
      },
      {
        _source: {
          id: 2,
          headline: 'Flood warning',
          publishedOn: '2026-10-20T10:00:00Z',
          otherSource: 'XYZ',
          mediaType: { name: 'Online' },
          tonePools: [],
        },
      },
    ],
  },
};

const settings: IFilterSettingsModel = {
  search: 'fire',
  size: 10,
  searchUnpublished: true,
  generatedPrompt: 'stories about fires',
};

const handlers = {
  onGenerate: vi.fn(),
  onChat: vi.fn(),
  onSearch: vi.fn(),
  onApply: vi.fn(),
  onClose: vi.fn(),
  onBuildQuery: (s: IFilterSettingsModel) => ({ query: { settings: s }, size: s.size }),
};

/** A stand-in for the app's form: one button that changes the size. */
const ManualForm: React.FC<IFilterAIManualFilterProps> = ({ settings, onChange }) => (
  <button type="button" onClick={() => onChange({ ...settings, size: 25 })}>
    set size 25
  </button>
);

const renderWorkbench = () =>
  render(
    <TestWrapper>
      <FilterAIWorkbench
        open
        prompt="stories about fires"
        editor="textarea"
        settings={settings}
        renderManualFilter={(props) => <ManualForm {...props} />}
        {...handlers}
      />
    </TestWrapper>,
  );

/** Lets the preview debounce elapse and the search resolve. */
const settle = () => act(async () => await vi.advanceTimersByTimeAsync(1000));

describe('Filter prompt workbench', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    handlers.onGenerate.mockReset();
    handlers.onSearch.mockReset().mockResolvedValue(stories);
    handlers.onApply.mockReset();
    handlers.onClose.mockReset();
  });
  afterEach(() => vi.useRealTimers());

  it('opens on the chat tab with the tabs in order, and lists published stories for the current filter', async () => {
    renderWorkbench();
    const tabs = screen.getAllByRole('tab', { hidden: true }).map((t) => t.textContent);
    expect(tabs).toEqual(['Chat', 'Result', 'Manual Filter', 'Elasticsearch Query']);
    expect(screen.getByRole('tab', { hidden: true, name: 'Chat' })).toHaveAttribute(
      'aria-selected',
      'true',
    );

    await settle();

    expect(handlers.onSearch).toHaveBeenCalledTimes(1);
    const [query, searchUnpublished] = handlers.onSearch.mock.calls[0];
    expect(searchUnpublished).toBe(false);
    expect(query.size).toBe(10);
    expect(query._source).toContain('headline');
    expect(query.query.settings.search).toBe('fire');

    const list = screen.getByRole('listbox', { hidden: true });
    expect(within(list).getByText('Fire in town')).toBeInTheDocument();
    expect(within(list).getByText('CBC')).toBeInTheDocument();
    expect(within(list).getByText('XYZ')).toBeInTheDocument();
    expect(within(list).getByText('2026-10-21')).toBeInTheDocument();
    expect(screen.getByText('stories')).toBeInTheDocument();
  });

  it('searches all content when the published-only switch is turned off', async () => {
    renderWorkbench();
    await settle();
    expect(handlers.onSearch).toHaveBeenCalledTimes(1);

    fireEvent.click(screen.getByText('Published content only'));
    await settle();

    expect(handlers.onSearch).toHaveBeenCalledTimes(2);
    expect(handlers.onSearch.mock.calls[1][1]).toBe(true);
  });

  it('generates with the switch applied, keeps the active tab, shows the search criteria, and leaves out the tool steps', async () => {
    handlers.onGenerate.mockResolvedValue({
      settings: { search: 'wildfire', size: 25, sourceIds: [3] },
      explanation: 'Wildfire stories from source 3.',
      steps: [{ tool: 'search', arguments: '{}', result: '{"total":1}' }],
    });
    renderWorkbench();
    await settle();

    fireEvent.click(screen.getByRole('button', { hidden: true, name: /Generate/ }));
    // Let the generation resolve, then the preview search run.
    await act(async () => {});
    await settle();

    expect(handlers.onGenerate).toHaveBeenCalledTimes(1);
    expect(handlers.onGenerate.mock.calls[0][0].settings.searchUnpublished).toBe(false);
    expect(handlers.onGenerate.mock.calls[0][0].prompt).toBe('stories about fires');
    // Generating leaves the user on the tab they were using.
    expect(screen.getByRole('tab', { hidden: true, name: 'Chat' })).toHaveAttribute(
      'aria-selected',
      'true',
    );
    fireEvent.click(screen.getByRole('tab', { hidden: true, name: 'Result' }));
    expect(screen.getByText('Search Criteria')).toBeInTheDocument();
    expect(screen.getByText('Wildfire stories from source 3.')).toBeInTheDocument();
    expect(screen.queryByText('How the AI built it')).not.toBeInTheDocument();
    expect(screen.queryByText('{"total":1}')).not.toBeInTheDocument();
    // The stories are listed for the generated filter.
    const last = handlers.onSearch.mock.calls[handlers.onSearch.mock.calls.length - 1][0];
    expect(last.query.settings.search).toBe('wildfire');
    expect(last.size).toBe(25);
  });

  it('applies manual changes made on the manual filter tab', async () => {
    renderWorkbench();
    await settle();
    expect(screen.getByRole('button', { hidden: true, name: 'Apply filter' })).toBeDisabled();

    fireEvent.click(screen.getByRole('tab', { hidden: true, name: 'Manual Filter' }));
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'set size 25' }));
    await settle();

    const last = handlers.onSearch.mock.calls[handlers.onSearch.mock.calls.length - 1][0];
    expect(last.size).toBe(25);

    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Apply filter' }));
    expect(handlers.onApply).toHaveBeenCalledTimes(1);
    const [result, prompt, query] = handlers.onApply.mock.calls[0];
    expect(result.settings.size).toBe(25);
    expect(result.settings.searchUnpublished).toBe(false);
    expect(prompt).toBe('stories about fires');
    expect(query).toBeUndefined();
    expect(handlers.onClose).toHaveBeenCalled();
  });

  it('asks why a selected story is included, sending the story and the query to the chat', async () => {
    handlers.onChat.mockReset().mockResolvedValue({
      answer: 'It matched the keyword fire.',
      nonce: 'abc',
      messages: [],
      steps: [],
    });
    renderWorkbench();
    await settle();

    fireEvent.click(screen.getByText('Fire in town'));
    expect(screen.getByRole('option', { hidden: true, name: /Fire in town/ })).toHaveAttribute(
      'aria-selected',
      'true',
    );
    fireEvent.click(
      screen.getAllByRole('button', { hidden: true, name: 'Why is it included?' })[0],
    );
    await act(async () => {});

    expect(screen.getByRole('tab', { hidden: true, name: 'Chat' })).toHaveAttribute(
      'aria-selected',
      'true',
    );
    expect(handlers.onChat).toHaveBeenCalledTimes(1);
    const request = handlers.onChat.mock.calls[0][0];
    expect(request.question).toBe('Why is this story included in the results?');
    expect(request.contentId).toBe(1);
    expect(request.query.query.settings.search).toBe('fire');
    expect(screen.getByText('It matched the keyword fire.')).toBeInTheDocument();
    expect(screen.getByText('About: Fire in town')).toBeInTheDocument();

    // The follow-up about excluding it keeps the story.
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'How do I exclude it?' }));
    await act(async () => {});
    expect(handlers.onChat.mock.calls[1][0].contentId).toBe(1);
    expect(handlers.onChat.mock.calls[1][0].question).toMatch(/exclude this story/);

    // Clearing the selection drops the story from later questions.
    fireEvent.click(screen.getAllByRole('button', { hidden: true, name: 'Clear' })[0]);
    expect(screen.queryByText(/Asking about/)).not.toBeInTheDocument();
  });
  it('closes an unchanged workbench without asking to apply', () => {
    renderWorkbench();
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Close' }));
    expect(handlers.onClose).toHaveBeenCalledTimes(1);
    expect(screen.queryByText('Apply the current prompt before closing?')).not.toBeInTheDocument();
  });

  it('asks before discarding a changed prompt and can keep editing or discard', () => {
    renderWorkbench();
    fireEvent.change(screen.getByLabelText('Draft prompt'), { target: { value: 'Flood stories' } });
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Close' }));
    expect(handlers.onClose).not.toHaveBeenCalled();
    expect(screen.getByText('Apply the current prompt before closing?')).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Keep editing' }));
    expect(screen.getByLabelText('Draft prompt')).toHaveValue('Flood stories');
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Close' }));
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Discard and close' }));
    expect(handlers.onClose).toHaveBeenCalledTimes(1);
    expect(handlers.onApply).not.toHaveBeenCalled();
  });

  it('generates the changed prompt before applying and closing', async () => {
    handlers.onGenerate.mockResolvedValue({
      settings: { ...settings, search: 'flood' },
      explanation: '',
      steps: [],
    });
    renderWorkbench();
    fireEvent.change(screen.getByLabelText('Draft prompt'), { target: { value: 'Flood stories' } });
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Close' }));
    fireEvent.click(
      screen.getByRole('button', { hidden: true, name: 'Generate, apply and close' }),
    );
    await act(async () => {});
    expect(handlers.onApply).toHaveBeenCalledWith(
      expect.objectContaining({
        settings: expect.objectContaining({ search: 'flood', generatedPrompt: 'Flood stories' }),
      }),
      'Flood stories',
    );
    expect(handlers.onClose).toHaveBeenCalledTimes(1);
  });

  it('asks to apply an already generated filter without generating it twice', async () => {
    handlers.onGenerate.mockResolvedValue({ settings, explanation: '', steps: [] });
    renderWorkbench();
    fireEvent.click(screen.getByRole('button', { hidden: true, name: /Generate/ }));
    await act(async () => {});
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Close' }));
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Apply and close' }));
    await act(async () => {});
    expect(handlers.onGenerate).toHaveBeenCalledTimes(1);
    expect(handlers.onApply).toHaveBeenCalledTimes(1);
    expect(handlers.onClose).toHaveBeenCalledTimes(1);
  });

  it('keeps the workbench and prompt open when generation before closing fails', async () => {
    handlers.onGenerate.mockRejectedValue(new Error('Unable to generate'));
    renderWorkbench();
    fireEvent.change(screen.getByLabelText('Draft prompt'), { target: { value: 'Flood stories' } });
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Close' }));
    fireEvent.click(
      screen.getByRole('button', { hidden: true, name: 'Generate, apply and close' }),
    );
    await act(async () => {});
    expect(handlers.onClose).not.toHaveBeenCalled();
    expect(handlers.onApply).not.toHaveBeenCalled();
    expect(screen.getByLabelText('Draft prompt')).toHaveValue('Flood stories');
    expect(screen.getAllByText('Unable to generate').length).toBeGreaterThan(0);
  });
  it('asks to apply an ungenerated prompt even when it was not edited inside the workbench', () => {
    render(
      <TestWrapper>
        <FilterAIWorkbench
          open
          prompt="New prompt"
          editor="textarea"
          settings={settings}
          {...handlers}
        />
      </TestWrapper>,
    );
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Close' }));
    expect(screen.getByText('Apply the current prompt before closing?')).toBeInTheDocument();
    expect(
      screen.getByRole('button', { hidden: true, name: 'Generate, apply and close' }),
    ).toBeEnabled();
    expect(handlers.onClose).not.toHaveBeenCalled();
  });

  it('regenerates when the prompt changes after generating a draft', async () => {
    handlers.onGenerate.mockResolvedValue({ settings, explanation: '', steps: [] });
    renderWorkbench();
    fireEvent.click(screen.getByRole('button', { hidden: true, name: /Generate/ }));
    await act(async () => {});
    fireEvent.change(screen.getByLabelText('Draft prompt'), { target: { value: 'Flood stories' } });
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Apply filter' }));
    await act(async () => {});
    expect(handlers.onGenerate).toHaveBeenCalledTimes(2);
    expect(handlers.onGenerate.mock.calls[1][0].prompt).toBe('Flood stories');
    expect(handlers.onApply.mock.calls[0][0].settings.generatedPrompt).toBe('Flood stories');
  });

  it('clears a discarded result on reopening', async () => {
    handlers.onGenerate.mockResolvedValue({
      settings: { ...settings, search: 'discard me' },
      explanation: '',
      steps: [],
    });
    const content = (open: boolean) => (
      <TestWrapper>
        <FilterAIWorkbench
          open={open}
          prompt="stories about fires"
          editor="textarea"
          settings={settings}
          {...handlers}
        />
      </TestWrapper>
    );
    const view = render(content(true));
    fireEvent.click(screen.getByRole('button', { hidden: true, name: /Generate/ }));
    await act(async () => {});
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Close' }));
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Discard and close' }));
    view.rerender(content(false));
    view.rerender(content(true));
    expect(screen.getByRole('button', { hidden: true, name: 'Apply filter' })).toBeDisabled();
    fireEvent.click(screen.getByRole('button', { hidden: true, name: 'Close' }));
    expect(screen.queryByText('Apply the current prompt before closing?')).not.toBeInTheDocument();
  });
});
