import { createContext, ReactNode, useState } from 'react';
import { IFileReferenceModel } from 'tno-core';

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
  const [activeStream, setActiveStream] = useState<{ source: string; id: number }>({
    id: 0,
    source: '',
  });
  const [activeFileReference, setActiveFileReference] = useState<IFileReferenceModel>();
  return (
    <ContentListContext.Provider
      value={{
        viewOptions,
        setViewOptions,
        groupBy,
        setGroupBy,
        activeStream,
        setActiveStream,
        activeFileReference,
        setActiveFileReference,
      }}
    >
      {children}
    </ContentListContext.Provider>
  );
};
