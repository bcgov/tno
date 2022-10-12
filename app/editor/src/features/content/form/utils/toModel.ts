import { IContentModel, ITimeTrackingModel } from 'hooks/api-editor';
import moment from 'moment';

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
    productId: values.productId,
    licenseId: values.licenseId,
    ownerId: !!values.ownerId ? values.ownerId : undefined,
    seriesId: values.seriesId,
    otherSeries: values.otherSeries,
    sourceId: values.sourceId,
    otherSource: values.otherSource,
    headline: values.headline,
    page: values.page,
    summary: values.summary,
    body: values.body,
    actions: values.actions,
    categories: values.categories,
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
    printContent: !values.section
      ? undefined
      : {
          contentId: values.id,
          edition: values.edition,
          section: values.section,
          byline: values.byline,
        },
    publishedOn: moment(values.publishedOn).toISOString(),
    version: values.version,
  };
}
