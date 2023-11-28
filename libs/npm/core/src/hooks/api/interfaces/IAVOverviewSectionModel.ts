import { IAuditColumnsModel, IAVOverviewSectionItemModel, ISeriesModel, ISourceModel } from '.';

export interface IAVOverviewSectionModel extends IAuditColumnsModel {
  id: number;
  name: string;
  instanceId: number;
  sourceId?: number;
  source?: ISourceModel;
  otherSource: string;
  seriesId?: number;
  series?: ISeriesModel;
  anchors: string;
  startTime: string;
  sortOrder: number;
  items: IAVOverviewSectionItemModel[];
}
