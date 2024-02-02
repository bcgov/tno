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
};
