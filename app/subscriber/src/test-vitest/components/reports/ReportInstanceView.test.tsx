import { act, render, screen } from '@testing-library/react';
import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { ReportInstanceView } from '../../../features/my-reports/ReportInstanceView';

const state = vi.hoisted(() => ({
  output: { instanceId: 7, subject: 'First subject', body: 'First body' } as
    | { instanceId: number; subject: string; body: string }
    | undefined,
  view: vi.fn(),
}));
vi.mock('store/hooks', () => ({
  useApp: () => [{ requests: [] }],
  useReportInstances: () => [{ viewReportInstance: state.view }],
}));
vi.mock('store/slices', () => ({ useProfileStore: () => [{ reportOutput: state.output }] }));
vi.mock('../../../features/my-reports/styled', () => ({ ReportInstanceView: 'div' }));
vi.mock('tno-core', () => ({
  Col: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  Show: ({ visible, children }: { visible: boolean; children: React.ReactNode }) =>
    visible ? children : null,
  Loading: () => <div>Loading</div>,
  NotFound: () => <div>Not found</div>,
}));

describe('Report instance navigation', () => {
  beforeEach(() => {
    state.output = { instanceId: 7, subject: 'First subject', body: 'First body' };
    state.view.mockReset().mockResolvedValue({});
  });

  it('hides the previous preview while loading another instance', async () => {
    const { rerender } = render(<ReportInstanceView instanceId={7} />);
    expect(screen.getByText('First body')).toBeInTheDocument();
    rerender(<ReportInstanceView instanceId={8} />);
    expect(screen.queryByText('First subject')).not.toBeInTheDocument();
    expect(screen.queryByText('First body')).not.toBeInTheDocument();
    await act(async () => {});
    expect(state.view).toHaveBeenCalledWith(8, undefined);
    state.output = { instanceId: 8, subject: 'Second subject', body: 'Second body' };
    rerender(<ReportInstanceView instanceId={8} />);
    expect(screen.getByText('Second body')).toBeInTheDocument();
    expect(state.view).toHaveBeenCalledTimes(1);
  });

  it('shows not found without retrying a missing instance after clearing the old cache', async () => {
    state.view.mockImplementation(async () => {
      state.output = undefined;
      return undefined;
    });
    await act(async () => {
      render(<ReportInstanceView instanceId={8} />);
    });
    expect(screen.getByText('Not found')).toBeInTheDocument();
    expect(state.view).toHaveBeenCalledTimes(1);
  });
});
