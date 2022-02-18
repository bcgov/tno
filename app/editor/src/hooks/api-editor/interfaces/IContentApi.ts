// TO:DO interface for form and api
export interface IContentApi {
  seriesId?: any;
  createdOn?: string | undefined;
  id?: number;
  status: number;
  contentTypeId: number;
  mediaTypeId: number;
  headline: string;
  summary: string;
  source: string;
  licenseId: number;
  page: string;
  section: string;
  transcription: string;
  ownerId: number;
}
