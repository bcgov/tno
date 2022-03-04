import { IContentModel } from 'hooks';
import moment from 'moment';

import { IContentForm } from '../interfaces';

export function toForm(model: IContentModel): IContentForm {
  // return form values in valid API format on submit of ContentForm
  // not utilized properly right now - update coming
  return {
    id: model.id,
    uid: model.uid ?? '',
    sourceUrl: model.sourceUrl ?? '',
    status: model.status,
    workflowStatus: model.workflowStatus,
    summary: model.summary,
    transcription: model.transcription ?? '',
    contentTypeId: model.contentTypeId,
    mediaTypeId: model.mediaTypeId,
    licenseId: model.licenseId,
    headline: model.headline,
    source: model.source,
    page: model.page,
    ownerId: model.ownerId,
    seriesId: model.seriesId,
    publishedOn: moment(model.publishedOn).format('MM-dd-YYYY HH:mm:ss'),
    // Print Content
    section: model.printContent?.section ?? '',
    edition: model.printContent?.edition ?? '',
    storyType: model.printContent?.storyType ?? '',
    byline: model.printContent?.byline ?? '',
  };
}
