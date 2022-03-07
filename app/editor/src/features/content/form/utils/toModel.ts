import { IContentModel } from 'hooks';
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
    licenseId: values.licenseId,
    ownerId: values.ownerId,
    seriesId: values.seriesId,
    headline: values.headline,
    source: values.source,
    page: values.page,
    summary: values.summary,
    transcription: values.transcription,
    publishedOn: moment(values.publishedOn).toDate(), // TODO: If they haven't set the publishedOn it will cause an error.
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
