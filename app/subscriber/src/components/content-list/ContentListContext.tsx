import { createContext, ReactNode, useState } from 'react';

import { defaultValueListContext } from './constants';
import { IContentListContext, IGroupByState, IToggleStates } from './interfaces';

export const ContentListContext = createContext<IContentListContext>(defaultValueListContext);

export interface IContentListProviderProps {
  children: ReactNode;
}
export const ContentListProvider: React.FC<IContentListProviderProps> = ({ children }) => {
  const [viewOptions, setViewOptions] = useState<IToggleStates>({
    date: false,
    section: true,
    checkbox: true,
    teaser: true,
    sentiment: true,
  });
  const [groupBy, setGroupBy] = useState<IGroupByState>('source');
  return (
    <ContentListContext.Provider value={{ viewOptions, setViewOptions, groupBy, setGroupBy }}>
      {children}
    </ContentListContext.Provider>
  );
};
