import { ContentStatusName, ContentTypeName } from 'hooks/api-editor';

import { IContentForm } from '../interfaces';

export const defaultFormValues = (contentType: ContentTypeName): IContentForm => {
  return {
    id: 0,
    uid: '',
    sourceUrl: '',
    status: ContentStatusName.Draft,
    contentType: contentType,
    sourceId: undefined,
    otherSource: '',
    tempSource: '',
    productId: 0,
    licenseId: 1,
    seriesId: undefined,
    otherSeries: '',
    ownerId: 0,
    page: '',
    headline: '',
    summary: '',
    body: '',
    publishedOn: '',
    publishedOnTime: '',
    actions: [],
    categories: [],
    tags: [],
    labels: [],
    tone: '',
    tonePools: [],
    timeTrackings: [],
    fileReferences: [],
    links: [],
    workOrders: [],
    // Print Content
    section: '',
    edition: '',
    byline: '',
  };
};
