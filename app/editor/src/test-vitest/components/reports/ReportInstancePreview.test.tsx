import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import ReportInstancePreview from '../../../features/reports/ReportInstancePreview';

const state = vi.hoisted(() => ({ id: '31', view: vi.fn(), report: vi.fn() }));
vi.mock('react-router-dom', () => ({ useParams: () => ({ id: state.id }) }));
vi.mock('store/hooks', () => ({
  useReports: () => [{ getReport: state.report }],
  useReportInstances: () => [{ viewReportInstance: state.view }],
  useApp: () => [{}],
  useSettings: () => ({}),
  useApiHub: () => ({ useHubEffect: () => {} }),
}));
vi.mock('store/hooks/admin', () => ({
  useUsers: () => [{}, { getDistributionListById: vi.fn() }],
}));
vi.mock('../../../features/reports/styled', () => ({ ReportPreview: 'div' }));
vi.mock('tno-core', () => ({
  AISectionStatusName: { Generating: 'Generating' },
  MessageTargetKey: { ReportStatus: 'ReportStatus' },
  ButtonVariant: { link: 'link' },
  UserAccountTypeName: { Distribution: 'Distribution' },
  Col: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  Button: ({ children }: { children: React.ReactNode }) => <button>{children}</button>,
  Show: ({ visible, children }: { visible: boolean; children: React.ReactNode }) =>
    visible ? <>{children}</> : null,
  Loading: () => <span>Loading</span>,
  ReportPreviewStatus: () => null,
}));

describe('Editor report size feedback', () => {
  beforeEach(() => {
    state.id = '31';
    state.view.mockReset();
    state.report.mockReset().mockResolvedValue({ id: 12 });
  });

  it('shows the reduction instructions instead of stale output and recovers on a successful preview', async () => {
    state.view.mockResolvedValueOnce({ reportId: 12, subject: 'Old subject', body: 'Old body' });
    const { rerender } = render(<ReportInstancePreview />);
    await screen.findByText('Old body');
    state.view.mockRejectedValueOnce({
      response: {
        data: { type: 'ReportTooLargeException', error: 'Remove stories and refresh the report.' },
      },
    });
    state.id = '32';
    rerender(<ReportInstancePreview />);
    expect(await screen.findByRole('alert')).toHaveTextContent('Remove stories and refresh');
    expect(screen.queryByText('Old body')).not.toBeInTheDocument();
    state.view.mockResolvedValueOnce({ reportId: 12, subject: 'Reduced', body: 'Smaller report' });
    state.id = '33';
    rerender(<ReportInstancePreview />);
    await screen.findByText('Smaller report');
    await waitFor(() => expect(screen.queryByRole('alert')).not.toBeInTheDocument());
  });
});
