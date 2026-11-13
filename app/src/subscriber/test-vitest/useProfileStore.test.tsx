import { configureStore } from '@reduxjs/toolkit';
import { renderHook } from '@testing-library/react';
import React from 'react';
import { Provider } from 'react-redux';
import { describe, expect, it } from 'vitest';

import { IReportModel } from '@/shared';

// The store module loads first, as it does in the app; the reducers import from it.
import '@/subscriber/store';

import { reducer } from '@/subscriber/store/rootReducer';
import { useProfileStore } from '@/subscriber/store/slices';

const report = (id: number, name = `Report ${id}`) => ({ id, name, instances: [] }) as any;

const setup = () => {
  const store = configureStore({
    reducer,
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware({ serializableCheck: false, immutableCheck: false }),
  });
  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <Provider store={store}>{children}</Provider>
  );
  return { store, hook: renderHook(useProfileStore, { wrapper }) };
};

describe('useProfileStore', () => {
  it('applies an update to the reports as they are when it is made, not as they were when it started', () => {
    // Opening a report directly asks for the list and for the report at the same time. Both
    // requests start while the store is empty, and the report arrives after the list.
    const { store, hook } = setup();
    const [, { storeMyReports }] = hook.result.current;
    const list = [report(257), report(463), report(512)];

    // The list arrives (as 'findMyReports' stores it).
    storeMyReports((reports) =>
      list.map((r) => ({ ...r, instances: reports.find((s) => s.id === r.id)?.instances ?? [] })),
    );
    // Then the report (as 'getReport' stores it), from the same out-of-date render.
    const opened = report(463, 'Report 463 with sections');
    storeMyReports((reports: IReportModel[]) =>
      reports.some((r) => r.id === opened.id)
        ? reports.map((r) => (r.id === opened.id ? opened : r))
        : [opened, ...reports],
    );

    expect(store.getState().profile.myReports).toEqual([list[0], opened, list[2]]);
  });

  it('still accepts a value in place of an update', () => {
    const { store, hook } = setup();
    const [, { storeMyReports }] = hook.result.current;

    storeMyReports([report(1)]);
    storeMyReports([report(2)]);

    expect(store.getState().profile.myReports).toEqual([report(2)]);
  });
});
