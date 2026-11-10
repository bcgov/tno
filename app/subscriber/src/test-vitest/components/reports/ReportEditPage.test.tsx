import { render, screen, waitFor } from '@testing-library/react';
import React from 'react';
import { IReportModel, ReportStatusName } from 'tno-core';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { defaultReport } from '../../../features/my-reports/constants';
import { ReportEditPage } from '../../../features/my-reports/edit/ReportEditPage';
import { IReportForm } from '../../../features/my-reports/interfaces';

const state = vi.hoisted(() => ({
  userInfo: { id: 1 },
  myReports: [] as unknown[],
  findMyReports: vi.fn(),
  getReport: vi.fn(),
  generateReport: vi.fn(),
  /** Every report the page has put in its form. */
  forms: [] as { id: number; charts: number[] }[],
}));
vi.mock('react-router-dom', () => ({
  useParams: () => ({ id: '15' }),
  useNavigate: () => vi.fn(),
}));
vi.mock('react-toastify', () => ({ toast: { error: vi.fn(), success: vi.fn() } }));
vi.mock('store/hooks', () => ({
  useApp: () => [{ userInfo: state.userInfo }],
  useApiHub: () => ({ useHubEffect: () => {} }),
  useReportInstances: () => [{ getReportInstance: vi.fn() }],
  useReports: () => [
    {},
    {
      generateReport: state.generateReport,
      getReport: state.getReport,
      findMyReports: state.findMyReports,
      addReport: vi.fn(),
      updateReport: vi.fn(),
      updateReportSubscribers: vi.fn(),
    },
  ],
  useReportTemplates: () => [{ getReportTemplate: vi.fn() }],
  useSettings: () => ({ defaultReportTemplateId: 1 }),
}));
vi.mock('store/slices', () => ({
  useProfileStore: () => [{ myReports: state.myReports }, { storeReportOutput: vi.fn() }],
}));
vi.mock('components/formik', () => ({
  FormikForm: ({ initialValues }: { initialValues: IReportForm }) => {
    const charts = initialValues.sections.map((s) => s.chartTemplates.length);
    state.forms.push({ id: initialValues.id, charts });
    return <div data-testid="charts">{`${initialValues.id}:${charts.join(',')}`}</div>;
  },
}));
vi.mock('components/section', () => ({ PageSection: () => null }));
vi.mock('../../../features/my-reports/edit/content', () => ({ ContentEditForm: () => null }));
vi.mock('../../../features/my-reports/edit/ReportEditContext', () => ({
  ReportEditContextProvider: () => null,
}));
vi.mock('../../../features/my-reports/edit/ReportEditForm', () => ({
  ReportEditForm: () => null,
}));
vi.mock('../../../features/my-reports/edit/view', () => ({ ReportContainer: () => null }));
vi.mock('../../../features/my-reports/edit/styled', () => ({ ReportEditPage: 'div' }));

const report = (chartIds: number[]): IReportModel =>
  ({
    ...defaultReport(1, 1),
    id: 15,
    name: 'Morning Report',
    sections: [
      {
        id: 1,
        name: 'analytics',
        settings: {},
        chartTemplates: chartIds.map((id, sortOrder) => ({ id, sortOrder, sectionSettings: {} })),
      },
    ],
    instances: [{ id: 7, reportId: 15, status: ReportStatusName.Pending, content: [] }],
  } as unknown as IReportModel);

/** The report as the list returned it before it carried section charts, with an instance merged in. */
const staleStoreReport = report([]);
/** The report as the API returns it by id. */
const savedReport = report([1, 2]);

describe('the report edit page fills its form from the API, not from the store', () => {
  beforeEach(() => {
    state.forms = [];
    state.myReports = [];
    state.findMyReports.mockReset().mockResolvedValue([staleStoreReport]);
    state.getReport.mockReset().mockResolvedValue(savedReport);
    state.generateReport.mockReset().mockResolvedValue(savedReport);
  });

  it('fetches the report even when the store already holds it with an instance', async () => {
    state.myReports = [staleStoreReport];
    render(<ReportEditPage />);

    await waitFor(() => expect(screen.getByTestId('charts')).toHaveTextContent('15:2'));
    expect(state.getReport).toHaveBeenCalledWith(15, false);
    // The copy in the store has no charts; saving a form filled from it would delete them.
    expect(state.forms.filter((f) => f.id === 15 && f.charts[0] !== 2)).toEqual([]);
    expect(state.generateReport).not.toHaveBeenCalled();
  });

  it('does not fill the form from the report list when the store is empty', async () => {
    render(<ReportEditPage />);

    await waitFor(() => expect(screen.getByTestId('charts')).toHaveTextContent('15:2'));
    expect(state.findMyReports).toHaveBeenCalledTimes(1);
    expect(state.getReport).toHaveBeenCalledWith(15, false);
    expect(state.forms.filter((f) => f.id === 15 && f.charts[0] !== 2)).toEqual([]);
    expect(state.generateReport).not.toHaveBeenCalled();
  });
});
