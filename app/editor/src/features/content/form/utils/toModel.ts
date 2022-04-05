import { IContentModel, ITimeTrackingModel, ITonePoolModel } from 'hooks/api-editor';
import moment from 'moment';

import { IContentForm } from '../interfaces';

export function toModel(values: IContentForm, tonePool?: ITonePoolModel): IContentModel {
  // return form values in valid API format on submit of ContentForm
  // not utilized properly right now - update coming
  return {
    id: values.id,
    uid: values.uid,
    sourceUrl: values.sourceUrl,
    status: values.status,
    workflowStatus: values.workflowStatus,
    contentTypeId: values.contentTypeId,
    mediaTypeId: values.mediaTypeId,
    licenseId: values.licenseId,
    ownerId: values.ownerId,
    seriesId: values.seriesId,
    otherSeries: values.otherSeries,
    dataSourceId: values.dataSourceId,
    source: values.source,
    headline: values.headline,
    page: values.page,
    summary: values.summary,
    transcription: values.transcription,
    actions: values.actions,
    categories: values.categories,
    tags: values.tags,
    timeTrackings: values.timeTrackings.map((x) => {
      const container = {} as ITimeTrackingModel;
      container.userId = x.userId;
      container.activity = x.activity;
      container.effort = x.effort;
      return container;
    }),
    tonePools: !!tonePool ? [{ ...tonePool, value: +values.tone }] : [],
    fileReferences: values.fileReferences,
    links: values.links,
    printContent: !values.section
      ? undefined
      : {
          contentId: values.id,
          edition: values.edition,
          section: values.section,
          storyType: values.storyType,
          byline: values.byline,
        },
    publishedOn: moment(values.publishedOn).toDate(),
    version: values.version,
  };
}
