export interface IEveningOverviewItem {
  itemType: number | string;
  time: string;
  sortOrder: number;
  summary: string;
  contentId?: string;
  id?: number;
  avOverviewSectionId?: number;
}
