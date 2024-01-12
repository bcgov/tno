import moment from 'moment';
import { ContentStatusName, ContentTypeName } from 'tno-core';

import { IContentForm } from '../interfaces';

export const defaultFormValues = (contentType: ContentTypeName): IContentForm => {
  return {
    id: 0,
    uid: '',
    sourceUrl: '',
    status: ContentStatusName.Draft,
    contentType: contentType,
    sourceId: '',
    otherSource: '',
    tempSource: '',
    mediaTypeId: 0,
    mediaType: undefined,
    licenseId: 1,
    seriesId: '',
    otherSeries: '',
    ownerId: 0,
    owner: undefined,
    contributorId: '',
    page: '',
    headline: '',
    summary: '',
    body: '',
    publishedOn: '',
    publishedOnTime: '',
    isHidden: false,
    isApproved: contentType !== ContentTypeName.AudioVideo,
    isPrivate: false,
    actions: [],
    topics: [],
    tags: [],
    labels: [],
    tonePools: [],
    timeTrackings: [],
    fileReferences: [],
    links: [],
    workOrders: [],
    versions: {},
    // Print Content
    section: '',
    edition: '',
    byline: '',
    createdOn: moment().toString(),
  };
};
