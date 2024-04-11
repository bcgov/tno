import moment from 'moment';
import { ContentStatusName, ContentTypeName } from 'tno-core';

import { IContentForm } from '../interfaces';

export const defaultFormValues = (contentType: ContentTypeName): IContentForm => {
  const publishedOn = moment();
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
    publishedOn: publishedOn.format('MMM D, yyyy HH:mm:ss'),
    publishedOnTime: publishedOn.format('HH:mm:ss'),
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
    quotes: [],
    userNotifications: [],
    versions: {},
    // Print Content
    section: '',
    edition: '',
    byline: '',
    createdOn: publishedOn.toString(),
  };
};
