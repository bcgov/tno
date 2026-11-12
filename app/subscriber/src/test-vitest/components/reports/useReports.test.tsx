import { renderHook } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { useReports } from '../../../store/hooks/subscriber/useReports';

const state = vi.hoisted(() => ({
  api: { findMyReports: vi.fn() },
  http: { get: vi.fn() },
  profile: { myReports: [] as any[] },
  store: { storeMyReports: vi.fn(), storeReportContent: vi.fn() },
  dispatch: vi.fn(async (_: string, request: () => Promise<unknown>, ...rest: unknown[]) =>
    request(),
  ),
}));
vi.mock('store/hooks', () => ({ useAjaxWrapper: () => state.dispatch }));
vi.mock('store/slices', () => ({ useProfileStore: () => [state.profile, state.store] }));
vi.mock('tno-core', () => ({
  sortable: () => 0,
  useApi: () => state.http,
  useApiSubscriberReports: () => state.api,
}));

const reports = [{ id: 1, name: 'Morning Report', instances: [] as any[] }];

/** The list the hook put in the store, given the list that was there. */
const stored = (current: any[] = []) => {
  const update = state.store.storeMyReports.mock.lastCall?.[0];
  return typeof update === 'function' ? update(current) : update;
};

describe('Stale list of reports', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    state.profile.myReports = [];
  });

  it('fetches the list, then only asks whether it has changed', async () => {
    state.api.findMyReports.mockResolvedValue({
      status: 200,
      data: reports,
      headers: { etag: '"5-abc"' },
    });
    await expect(renderHook(useReports).result.current[1].findMyReports()).resolves.toBe(reports);
    expect(stored()).toEqual(reports);
    expect(state.http.get).not.toHaveBeenCalled();

    // The page still holds the list, and the API says it is the latest.
    state.profile.myReports = reports;
    state.http.get.mockRejectedValue({ response: { status: 304 } });
    await expect(renderHook(useReports).result.current[1].findMyReports()).resolves.toBe(reports);
    expect(state.http.get).toHaveBeenCalledWith('/subscriber/reports/my-reports', {
      headers: { 'If-None-Match': '"5-abc"' },
    });
    expect(state.store.storeMyReports).toHaveBeenCalledTimes(1);
    // The check runs behind the page.
    expect(state.dispatch.mock.calls[1][3]).toBe(true);
  });

  it('replaces the list when the API has a newer one', async () => {
    const newer = [
      { id: 1, name: 'Morning Report (renamed)', instances: [] },
      { id: 2, name: 'Evening Report', instances: [] },
    ];
    // Opening a report's card loaded its instances, which the list does not carry.
    const current = [{ ...reports[0], instances: [{ id: 31 }] }];
    state.profile.myReports = current;
    state.http.get.mockResolvedValue({ status: 200, data: newer, headers: { etag: '"5-def"' } });

    await expect(renderHook(useReports).result.current[1].findMyReports()).resolves.toBe(newer);
    expect(stored(current)).toEqual([{ ...newer[0], instances: [{ id: 31 }] }, newer[1]]);

    state.profile.myReports = newer;
    state.http.get.mockRejectedValue({ response: { status: 304 } });
    await renderHook(useReports).result.current[1].findMyReports();
    expect(state.http.get).toHaveBeenLastCalledWith('/subscriber/reports/my-reports', {
      headers: { 'If-None-Match': '"5-def"' },
    });
  });

  it('fetches the whole list when the page no longer holds one', async () => {
    state.api.findMyReports.mockResolvedValue({
      status: 200,
      data: reports,
      headers: { etag: '"5-def"' },
    });

    await renderHook(useReports).result.current[1].findMyReports();

    expect(state.http.get).not.toHaveBeenCalled();
    expect(stored()).toEqual(reports);
  });

  it('still reports a failure to fetch the list', async () => {
    const error = { response: { status: 500 } };
    state.profile.myReports = reports;
    state.http.get.mockRejectedValue(error);

    await expect(renderHook(useReports).result.current[1].findMyReports()).rejects.toBe(error);
  });
});
