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
  frontpageImageMediaTypeId?: number;
  frontpageFilterId?: number;
  morningReportId?: number;
  frontPageImagesReportId?: number;
  topStoryAlertId?: number;
  excludeBylineIds?: number[];
  excludeSourceIds?: number[];
}
