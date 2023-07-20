export interface IEveningOverviewItem {
  itemType: string;
  time: string;
  sortOrder: number;
  summary: string;
  contentId?: string;
  id: number;
  avOverviewSectionId?: number;
}
