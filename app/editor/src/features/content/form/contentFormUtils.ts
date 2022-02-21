import { IContentApi } from 'hooks';

export function formContentToApiContent(formValues: IContentApi): IContentApi {
  // return form values in valid API format on submit of ContentForm
  // not utilized properly right now - update coming
  return {
    status: 0,
    id: !!formValues.id ? formValues.id : 0,
    contentTypeId: 1,
    headline: formValues.headline,
    source: formValues.source,
    licenseId: 1,
    mediaTypeId: formValues.mediaTypeId,
    page: formValues.page,
    section: 'section',
    summary: formValues.summary,
    transcription: formValues.transcription,
    // createdOn: formValues.createdOn && formValues.createdOn,
    ownerId: 1,
    // seriesId: formValues.seriesId,
  };
}
