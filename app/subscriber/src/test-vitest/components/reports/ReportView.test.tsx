import { act, render, screen } from '@testing-library/react';
import React from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { ReportView } from '../../../features/my-reports/edit/view/ReportView';

const state = vi.hoisted(() => ({
  requests: [] as { group: string[] }[],
  reportOutput: { instanceId: 7, aiSections: [{ status: 'NotStarted' }] } as {
    instanceId: number;
    aiSections?: { status: string }[];
    sizeError?: string;
    body?: string;
  },
  view: vi.fn(),
  store: vi.fn(),
  context: {
    values: { instances: [{ id: 7, updatedOn: 'today', sentOn: undefined }] },
    previewLastUpdatedOn: 'today',
    setPreviewLastUpdatedOn: vi.fn(),
  },
}));
vi.mock('store/hooks', () => ({
  useApp: () => [{ requests: state.requests }],
  useReportInstances: () => [{ viewReportInstance: state.view }],
  useApiHub: () => ({ useHubEffect: () => {} }),
}));
vi.mock('store/slices', () => ({
  useProfileStore: () => [{ reportOutput: state.reportOutput }, { storeReportOutput: state.store }],
}));
vi.mock('../../../features/my-reports/edit/ReportEditContext', () => ({
  useReportEditContext: () => state.context,
}));
vi.mock('../../../features/my-reports/edit/view/styled', () => ({ ReportView: 'div' }));
vi.mock('../../../features/my-reports/edit/view/ReportViewPromptButton', () => ({
  ReportViewPromptButton: () => null,
}));
vi.mock('tno-core', () => ({
  AISectionStatusName: { NotStarted: 'NotStarted', Generating: 'Generating' },
  MessageTargetKey: { ReportStatus: 'ReportStatus' },
  Col: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  Row: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  ReportPreviewStatus: () => null,
}));

describe('Report preview background refresh', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    state.requests = [];
    state.reportOutput = { instanceId: 7, aiSections: [{ status: 'NotStarted' }] };
    state.view.mockReset().mockResolvedValue({ body: 'Ready' });
    state.store.mockReset();
  });
  afterEach(() => vi.useRealTimers());

  it('refreshes queued work even when the initial response was still loading, and stops when ready', async () => {
    state.requests = [{ group: ['view-report'] }];
    const { rerender } = render(<ReportView />);
    await act(async () => {
      await vi.advanceTimersByTimeAsync(30000);
    });
    expect(state.view).not.toHaveBeenCalled();
    state.requests = [];
    rerender(<ReportView />);
    await act(async () => {
      await vi.advanceTimersByTimeAsync(30000);
    });
    expect(state.view).toHaveBeenCalledWith(7, true);
    state.reportOutput = { instanceId: 7, aiSections: [{ status: 'Ready' }] };
    rerender(<ReportView />);
    await act(async () => {
      await vi.advanceTimersByTimeAsync(60000);
    });
    expect(state.view).toHaveBeenCalledTimes(1);
  });

  it('checks running work without a hub notification and cancels on leaving the page', async () => {
    state.reportOutput.aiSections = [{ status: 'Generating' }];
    const { unmount } = render(<ReportView />);
    await act(async () => {
      await vi.advanceTimersByTimeAsync(30000);
    });
    expect(state.view).toHaveBeenCalledTimes(1);
    unmount();
    await act(async () => {
      await vi.advanceTimersByTimeAsync(60000);
    });
    expect(state.view).toHaveBeenCalledTimes(1);
  });

  it('does not repeatedly retry a failed section', async () => {
    state.reportOutput.aiSections = [{ status: 'Failed' }];
    render(<ReportView />);
    await act(async () => {
      await vi.advanceTimersByTimeAsync(60000);
    });
    expect(state.view).not.toHaveBeenCalled();
  });

  it('keeps size feedback visible, hides stale output, and clears it after a successful refresh', async () => {
    state.reportOutput = {
      instanceId: 7,
      body: 'Old report output',
      sizeError: 'This report has grown too large to run. Remove stories and refresh the report.',
    };
    const { rerender } = render(<ReportView />);
    expect(screen.getByRole('alert')).toHaveTextContent('Remove stories and refresh');
    expect(screen.queryByText('Old report output')).not.toBeInTheDocument();
    await act(async () => {
      await vi.advanceTimersByTimeAsync(60000);
    });
    expect(state.view).not.toHaveBeenCalled();
    state.reportOutput = { instanceId: 7, body: 'Reduced report' };
    rerender(<ReportView />);
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });

  it("does not show another instance's size error", () => {
    state.reportOutput = { instanceId: 8, sizeError: 'Another report is too large' };
    render(<ReportView />);
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });
});
