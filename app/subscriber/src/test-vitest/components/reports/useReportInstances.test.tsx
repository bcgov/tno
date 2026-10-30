import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { useReportInstances } from '../../../store/hooks/subscriber/useReportInstances';

const state = vi.hoisted(() => ({
  api: { viewReportInstance: vi.fn() },
  store: { storeMyReports: vi.fn(), storeReportContent: vi.fn(), storeReportOutput: vi.fn() },
  dispatch: vi.fn(async (_: string, request: () => Promise<unknown>) => request()),
}));
vi.mock('store/hooks', () => ({ useAjaxWrapper: () => state.dispatch }));
vi.mock('store/slices', () => ({ useProfileStore: () => [{}, state.store] }));
vi.mock('tno-core', () => ({ useApiSubscriberReportInstances: () => state.api }));

describe('Report size feedback', () => {
  beforeEach(() => vi.clearAllMocks());

  it('replaces a stale preview with actionable size feedback for the requested instance', async () => {
    const error = {
      response: {
        data: { type: 'ReportTooLargeException', error: 'Reduce this report and refresh.' },
      },
    };
    state.api.viewReportInstance.mockRejectedValue(error);
    const { result } = renderHook(useReportInstances);
    await expect(result.current[0].viewReportInstance(31, true)).rejects.toBe(error);
    expect(state.store.storeReportOutput).toHaveBeenCalledWith({
      instanceId: 31,
      reportId: 0,
      subject: '',
      body: '',
      sizeError: 'Reduce this report and refresh.',
    });
  });

  it('does not mislabel other errors and allows a corrected report to be requested again', async () => {
    const error = { response: { data: { type: 'NotAuthorizedException', error: 'Unauthorized' } } };
    state.api.viewReportInstance.mockRejectedValueOnce(error).mockResolvedValueOnce({
      data: { reportId: 12, instanceId: 31, subject: 'Report', body: 'Reduced content' },
    });
    const { result } = renderHook(useReportInstances);
    await expect(result.current[0].viewReportInstance(31)).rejects.toBe(error);
    expect(state.store.storeReportOutput).not.toHaveBeenCalled();
    await expect(result.current[0].viewReportInstance(31, true)).resolves.toMatchObject({
      body: 'Reduced content',
    });
  });

  it.each(['success', 'size error'])(
    'ignores a late %s from another viewer after a newer preview completes',
    async (outcome) => {
      let resolveOld!: (value: unknown) => void;
      let rejectOld!: (reason: unknown) => void;
      state.api.viewReportInstance
        .mockImplementationOnce(
          () =>
            new Promise((resolve, reject) => {
              resolveOld = resolve;
              rejectOld = reject;
            }),
        )
        .mockResolvedValueOnce({ data: { reportId: 2, subject: 'New report', body: 'New body' } });
      const first = renderHook(useReportInstances);
      const second = renderHook(useReportInstances);
      const oldRequest = first.result.current[0].viewReportInstance(31).catch(() => {});
      await second.result.current[0].viewReportInstance(32);
      if (outcome === 'success') resolveOld({ data: { reportId: 1, body: 'Old body' } });
      else
        rejectOld({
          response: { data: { type: 'ReportTooLargeException', error: 'Old size error' } },
        });
      await oldRequest;
      expect(state.store.storeReportOutput).toHaveBeenCalledTimes(1);
      expect(state.store.storeReportOutput).toHaveBeenLastCalledWith({
        instanceId: 32,
        reportId: 2,
        subject: 'New report',
        body: 'New body',
      });
    },
  );
});
