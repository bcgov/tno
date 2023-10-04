export interface IWorkOrderContentModel {
  contentId: number;
  headline: string;
  otherSource: string;
  isApproved: boolean;
  product?: string;
  series?: string;
  contributor?: string;
}
