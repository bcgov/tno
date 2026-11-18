export interface ILLMUsageItemModel {
  type: 'ReportSection' | 'AutomationProfile' | 'ContentAnalysis' | 'Search';
  id?: number;
  sectionId?: number;
  name: string;
  detail: string;
  owner?: string;
  isEnabled: boolean;
}
