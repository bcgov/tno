import moment from 'moment';
import { IContentModel } from 'tno-core';

import { IContentForm } from '../interfaces';

/**
 * Converts the api model into form values.
 * @param model The model from the api.
 * @returns A new instance of form values.
 */
export function toForm(model: IContentModel): IContentForm {
  // return form values in valid API format on submit of ContentForm
  // not utilized properly right now - update coming
  return {
    id: model.id,
    uid: model.uid ?? '',
    sourceUrl: model.sourceUrl ?? '',
    status: model.status,
    contentType: model.contentType,
    summary: model.summary,
    body: model.body ?? '',
    mediaTypeId: model.mediaTypeId,
    mediaType: model.mediaType,
    licenseId: model.licenseId,
    headline: model.headline,
    byline: model.byline ?? '',
    sourceId: model.sourceId ?? '',
    otherSource: model.otherSource,
    tempSource: !!model.sourceId ? '' : model.otherSource,
    page: model.page,
    ownerId: model.ownerId ?? '',
    owner: model.owner,
    seriesId: model.seriesId ?? '',
    contributorId: model.contributorId ?? '',
    otherSeries: '',
    postedOn: !!model.postedOn ? moment(model.postedOn).format('HH:mm:ss') : '',
    publishedOn: model.publishedOn ?? '',
    publishedOnTime: !!model.publishedOn ? moment(model.publishedOn).format('HH:mm:ss') : '',
    isHidden: model.isHidden,
    isApproved: model.isApproved,
    isPrivate: model.isPrivate,
    actions: model.actions ?? [],
    topics: model.topics ?? [],
    tags: model.tags ?? [],
    labels: model.labels ?? [],
    tonePools: model.tonePools ?? [],
    timeTrackings: model.timeTrackings ?? [],
    fileReferences: model.fileReferences ?? [],
    links: model.links ?? [],
    workOrders: [],
    versions: model.versions,
    quotes: model.quotes,
    userNotifications: model.userNotifications,
    // Print Content
    section: model.section ?? '',
    edition: model.edition ?? '',
    version: model.version,
  };
}
