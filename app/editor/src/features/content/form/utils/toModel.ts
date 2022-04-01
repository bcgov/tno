import { IContentModel, ITimeTrackingModel } from 'hooks/api-editor';
import moment from 'moment';

import { IContentForm } from '../interfaces';

export function toModel(values: IContentForm): IContentModel {
  // return form values in valid API format on submit of ContentForm
  // not utilized properly right now - update coming
  return {
    id: values.id,
    uid: !!values.uid ? values.uid : undefined,
    sourceUrl: !!values.sourceUrl ? values.sourceUrl : undefined,
    status: values.status,
    workflowStatus: values.workflowStatus,
    contentTypeId: values.contentTypeId,
    mediaTypeId: values.mediaTypeId,
    createdBy: 'placeholder',
    licenseId: values.licenseId,
    ownerId: values.ownerId,
    seriesId: values.seriesId,
    headline: values.headline,
    source: values.source,
    page: values.page,
    summary: values.summary,
    actions: !!values.actions ? values.actions : undefined,
    tags: !!values.tags ? values.tags : undefined,
    categories: values.categories,
    transcription: values.transcription,
    timeTrackings: !!values.timeTrackings
      ? values.timeTrackings.map((x) => {
          const container = {} as ITimeTrackingModel;
          container.userId = x.userId;
          container.activity = x.activity;
          container.effort = x.effort;
          return container;
        })
      : undefined,
    publishedOn: !!values.publishedOn ? moment(values.publishedOn).toDate() : new Date(), // TODO: If they haven't set the publishedOn it will cause an error.
    version: values.version,
    printContent: !values.section
      ? undefined
      : {
          contentId: values.id,
          edition: values.edition,
          section: values.section,
          storyType: values.storyType,
          byline: values.byline,
        },
  };
}
