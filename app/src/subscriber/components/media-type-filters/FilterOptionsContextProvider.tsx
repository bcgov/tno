import React, { createContext, ReactNode, useContext, useState } from 'react';

interface IFilterOptionsContext {
  hasProcessedInitialPreferences: boolean;
  setHasProcessedInitialPreferences: React.Dispatch<React.SetStateAction<boolean>>;
}

const FilterOptionContext = createContext<IFilterOptionsContext | undefined>(undefined);

export const useFilterOptionContext = (): IFilterOptionsContext => {
  const context = useContext(FilterOptionContext);
  if (!context) {
    throw new Error('FilterOptionsContext must be used within a FilterOptionsProvider');
  }
  return context;
};

interface IFilterOptionsProvider {
  children: ReactNode;
}

export const FilterOptionsProvider: React.FC<IFilterOptionsProvider> = ({ children }) => {
  const [hasProcessedInitialPreferences, setHasProcessedInitialPreferences] =
    useState<boolean>(false);

  return (
    <FilterOptionContext.Provider
      value={{ hasProcessedInitialPreferences, setHasProcessedInitialPreferences }}
    >
      {children}
    </FilterOptionContext.Provider>
  );
};
