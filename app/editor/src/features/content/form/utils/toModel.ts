import moment from 'moment';
import { IContentModel, ITimeTrackingModel } from 'tno-core';

import { IContentForm } from '../interfaces';

/**
 * Convert the form to a model so that it can be sent to the api.
 * @param values The form values.
 * @returns A model that can be sent to the api.
 */
export function toModel(values: IContentForm): IContentModel {
  // return form values in valid API format on submit of ContentForm
  // not utilized properly right now - update coming
  return {
    id: values.id,
    uid: values.uid,
    sourceUrl: values.sourceUrl,
    status: values.status,
    contentType: values.contentType,
    mediaTypeId: values.mediaTypeId,
    licenseId: values.licenseId,
    ownerId: !!values.ownerId ? values.ownerId : undefined,
    seriesId: !!values.seriesId ? values.seriesId : undefined,
    contributorId: !!values.contributorId ? values.contributorId : undefined,
    otherSeries: values.otherSeries,
    sourceId: !!values.sourceId ? values.sourceId : undefined,
    otherSource: values.otherSource,
    headline: values.headline,
    byline: values.byline,
    page: values.page,
    summary: values.summary,
    body: values.body,
    isHidden: values.isHidden,
    isApproved: values.isApproved,
    isPrivate: values.isPrivate,
    actions: values.actions,
    topics: values.topics,
    tags: values.tags,
    labels: values.labels,
    timeTrackings: values.timeTrackings.map((x) => {
      const container = {} as ITimeTrackingModel;
      container.userId = x.userId;
      container.activity = x.activity;
      container.effort = x.effort;
      return container;
    }),
    tonePools: values.tonePools,
    fileReferences: values.fileReferences,
    links: values.links,
    quotes: values.quotes,
    userNotifications: values.userNotifications,
    versions: values.versions,
    // Print Content
    edition: values.edition,
    section: values.section,
    postedOn: values.postedOn ? moment(values.publishedOn).toISOString() : undefined,
    publishedOn: moment(values.publishedOn).toISOString(),
    version: values.version,
  };
}
