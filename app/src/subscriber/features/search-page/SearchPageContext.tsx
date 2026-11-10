import React, { createContext, ReactNode, useContext, useState } from 'react';

type SearchPageContextType = {
  expanded: boolean;
  setExpanded: React.Dispatch<React.SetStateAction<boolean>>;
};

const SearchPageContext = createContext<SearchPageContextType | undefined>(undefined);

export function useSearchPageContext() {
  const context = useContext(SearchPageContext);
  if (context === undefined) {
    throw new Error('useSearchPageContext must be used within a MyProvider');
  }
  return context;
}

interface IProviderProps {
  children: ReactNode;
}

export function SearchPageProvider({ children }: IProviderProps) {
  const [expanded, setExpanded] = useState<boolean>(true);

  const value = { expanded, setExpanded };

  return <SearchPageContext.Provider value={value}>{children}</SearchPageContext.Provider>;
}
