import { act, render, screen } from '@testing-library/react';
import React from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { ReportView } from '../../../features/my-reports/edit/view/ReportView';

const state = vi.hoisted(() => ({
  routeId: '1',
  requests: [] as { group: string[] }[],
  reportOutput: { instanceId: 7, aiSections: [{ status: 'NotStarted' }] } as {
    instanceId: number;
    aiSections?: { status: string }[];
    sizeError?: string;
    body?: string;
    subject?: string;
  },
  view: vi.fn(),
  store: vi.fn(),
  context: {
    values: { id: 1, instances: [{ id: 7, updatedOn: 'today', sentOn: undefined }] },
    previewLastUpdatedOn: 'today',
    setPreviewLastUpdatedOn: vi.fn(),
  },
}));
vi.mock('react-router-dom', () => ({ useParams: () => ({ id: state.routeId }) }));
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
    state.routeId = '1';
    state.context.values = { id: 1, instances: [{ id: 7, updatedOn: 'today', sentOn: undefined }] };
    state.context.previewLastUpdatedOn = 'today';
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

  it('hides the previous subject and body and requests the new instance even with the same timestamp', async () => {
    state.reportOutput = { instanceId: 7, subject: 'First subject', body: 'First body' };
    const { rerender } = render(<ReportView />);
    expect(screen.getByText('First body')).toBeInTheDocument();
    state.view.mockRejectedValue(new Error('Preview unavailable'));
    state.context.values.instances[0].id = 8;
    rerender(<ReportView />);
    expect(screen.queryByText('First subject')).not.toBeInTheDocument();
    expect(screen.queryByText('First body')).not.toBeInTheDocument();
    await act(async () => {});
    expect(state.view).toHaveBeenCalledWith(8, true);
    expect(state.view).toHaveBeenCalledTimes(1);
    expect(screen.queryByText('First body')).not.toBeInTheDocument();
    state.reportOutput = { instanceId: 8, subject: 'Second subject', body: 'Second body' };
    rerender(<ReportView />);
    expect(screen.getByText('Second subject')).toBeInTheDocument();
    expect(screen.getByText('Second body')).toBeInTheDocument();
  });

  it('hides the old preview immediately while the next report form is still loading', () => {
    state.reportOutput = { instanceId: 7, body: 'First body' };
    const { rerender } = render(<ReportView />);
    state.routeId = '2';
    rerender(<ReportView />);
    expect(screen.queryByText('First body')).not.toBeInTheDocument();
    expect(state.view).not.toHaveBeenCalled();
  });

  it('shows no cached HTML when the selected report has no instance', () => {
    state.reportOutput = { instanceId: 7, body: 'First body' };
    state.context.values.instances = [];
    render(<ReportView />);
    expect(screen.queryByText('First body')).not.toBeInTheDocument();
    expect(state.view).not.toHaveBeenCalled();
  });
});
