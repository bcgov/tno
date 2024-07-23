import { createContext, ReactNode, useState } from 'react';
import React from 'react';
import { useApp, useReports } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { IFileReferenceModel } from 'tno-core';

import { defaultValueListContext } from './constants';
import { IContentListContext, IGroupByState, IToggleStates } from './interfaces';

export const ContentListContext = createContext<IContentListContext>(defaultValueListContext);

export interface IContentListProviderProps {
  children: ReactNode;
}
export const ContentListProvider: React.FC<IContentListProviderProps> = ({ children }) => {
  const [, { initialized }] = useApp();
  const [viewOptions, setViewOptions] = useState<IToggleStates>({
    date: false,
    section: true,
    checkbox: true,
    teaser: true,
    sentiment: true,
  });
  // Make a request to reports to determine what content is in a report.
  const [, { getAllContentInMyReports }] = useReports();
  const [, { storeReportContent }] = useProfileStore();

  const [groupBy, setGroupBy] = useState<IGroupByState>('source');
  const [activeStream, setActiveStream] = useState<{ source: string; id: number }>({
    id: 0,
    source: '',
  });
  const [activeFileReference, setActiveFileReference] = useState<IFileReferenceModel>();

  React.useEffect(() => {
    if (initialized) {
      getAllContentInMyReports()
        .then((reportContent) => {
          storeReportContent(reportContent);
        })
        .catch(() => {});
    }
    // Only interested in making this request when the page is initialized.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialized]);

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
