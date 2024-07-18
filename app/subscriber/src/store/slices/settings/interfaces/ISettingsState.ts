export interface ISettingsState {
  loadingState: number;
  isReady: boolean;
  commentaryActionId?: number;
  topStoryActionId?: number;
  featuredStoryActionId?: number;
  alertActionId?: number;
  editorUrl?: string;
  subscriberUrl?: string;
  defaultReportTemplateId?: number;
  frontpageFilterId?: number;
  frontPageImageMediaTypeId?: number;
  excludeBylineIds?: number[];
  excludeSourceIds?: number[];
  eventOfTheDayReportId?: number;
  mediaTypesIdsAllSources?: number[];
}
