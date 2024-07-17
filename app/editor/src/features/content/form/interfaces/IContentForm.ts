import { IContentModel, IQuoteModel, IWorkOrderModel } from 'tno-core';

export interface IContentForm
  extends Omit<
    IContentModel,
    | 'otherSeries'
    | 'sourceId'
    | 'otherSource'
    | 'seriesId'
    | 'contributorId'
    | 'ownerId'
    | 'uid'
    | 'sourceUrl'
  > {
  otherSeries: string | '';
  sourceId: number | '';
  otherSource: string | '';
  tempSource: string;
  seriesId: number | '';
  contributorId: number | '';
  ownerId: number | '';
  uid: string;
  sourceUrl: string;
  workOrders: IWorkOrderModel[];
  quotes: IQuoteModel[];
  publishedOnTime: string;
  postedOnTime?: string;
  file?: File | null;
  prep?: number;
  // Print Content
  showOther?: boolean;
}
