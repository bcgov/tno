export interface IWorkOrderContentModel {
  contentId: number;
  headline: string;
  otherSource: string;
  isApproved: boolean;
  mediaType?: string;
  series?: string;
  contributor?: string;
}
