import React from 'react';
import { useContent } from 'store/hooks';
import { IFilterSettingsModel } from 'tno-core';

/** The settings a search form shows, and how it changes them. */
export type SearchSettingsBinding = [
  IFilterSettingsModel,
  (settings: IFilterSettingsModel) => void,
];

/**
 * Binds the advanced search sections to settings other than the search page's own.
 * Without a provider the sections read and write the search filter in the store.
 */
export const SearchSettingsContext = React.createContext<SearchSettingsBinding | undefined>(
  undefined,
);

/**
 * The settings the advanced search sections show and change: the nearest provider's, or the
 * search filter in the store.
 * @returns The settings and the function that replaces them.
 */
export const useSearchSettings = (): SearchSettingsBinding => {
  const bound = React.useContext(SearchSettingsContext);
  const [
    {
      search: { filter },
    },
    { storeSearchFilter },
  ] = useContent();
  return bound ?? [filter, storeSearchFilter];
};
