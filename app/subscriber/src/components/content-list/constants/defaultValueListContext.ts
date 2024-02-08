import { IContentListContext } from '../interfaces';

export const defaultValueListContext: IContentListContext = {
  viewOptions: {
    date: false,
    section: true,
    checkbox: true,
    teaser: true,
    sentiment: true,
  },
  setViewOptions: () => {},
  groupBy: 'source',
  setGroupBy: () => {},
  activeStream: { id: 0, source: '' },
  setActiveStream: () => {},
  activeFileReference: undefined,
  setActiveFileReference: () => {},
};
