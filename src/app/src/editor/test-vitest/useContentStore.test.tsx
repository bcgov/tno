import { configureStore } from '@reduxjs/toolkit';
import { renderHook } from '@testing-library/react';
import React from 'react';
import { Provider } from 'react-redux';
import { describe, expect, it } from 'vitest';

// The store module loads first, as it does in the app; the reducers import from it.
import '@/editor/store';

import { reducer } from '@/editor/store/rootReducer';
import { useContentStore } from '@/editor/store/slices';

const setup = () => {
  const store = configureStore({
    reducer,
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware({ serializableCheck: false, immutableCheck: false }),
  });
  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <Provider store={store}>{children}</Provider>
  );
  return { store, hook: renderHook(() => useContentStore(), { wrapper }) };
};

describe('useContentStore', () => {
  it('applies each update to the filter as it is when it is made', () => {
    const { store, hook } = setup();
    // Both updates come from the same render, before it can see the first one.
    const [, { storeFilterAdvanced }] = hook.result.current;

    storeFilterAdvanced((filter) => ({ ...filter, startDate: '2026-11-01' }));
    storeFilterAdvanced((filter) => ({ ...filter, endDate: '2026-11-13' }));

    expect(store.getState().content.filterAdvanced).toMatchObject({
      startDate: '2026-11-01',
      endDate: '2026-11-13',
    });
  });

  it('updates the papers filter from the papers filter', () => {
    const { store, hook } = setup();
    const [, { storeFilterAdvanced, storeFilterPaperAdvanced }] = hook.result.current;

    storeFilterAdvanced((filter) => ({ ...filter, searchTerm: 'content' }));
    storeFilterPaperAdvanced((filter) => ({ ...filter, startDate: '2026-11-01' }));

    const { filterAdvanced, filterPaperAdvanced } = store.getState().content;
    expect(filterPaperAdvanced.startDate).toBe('2026-11-01');
    expect(filterPaperAdvanced.searchTerm).not.toBe('content');
    expect(filterAdvanced.startDate).toBeUndefined();
  });
});
