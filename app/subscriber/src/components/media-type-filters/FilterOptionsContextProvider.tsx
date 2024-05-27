import React, { createContext, ReactNode, useContext, useState } from 'react';

import { FilterOptionTypes } from './constants';

interface IFilterOptionsContext {
  active: FilterOptionTypes | undefined;
  setActive: React.Dispatch<React.SetStateAction<FilterOptionTypes | undefined>>;
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
  const [active, setActive] = useState<FilterOptionTypes | undefined>(undefined);

  return (
    <FilterOptionContext.Provider value={{ active, setActive }}>
      {children}
    </FilterOptionContext.Provider>
  );
};
