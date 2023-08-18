import { AVOverviewTemplateTypeName } from '../constants';
import {
  IAuditColumnsModel,
  IAVOverviewTemplateModel,
  IAVOverviewTemplateSectionItemModel,
  ISeriesModel,
  ISourceModel,
} from '.';

export interface IAVOverviewTemplateSectionModel extends IAuditColumnsModel {
  id: number;
  name: string;
  templateType: AVOverviewTemplateTypeName;
  template?: IAVOverviewTemplateModel;
  sourceId?: number;
  source?: ISourceModel;
  otherSource: string;
  seriesId?: number;
  series?: ISeriesModel;
  anchors: string;
  startTime: string;
  sortOrder: number;
  items: IAVOverviewTemplateSectionItemModel[];
}
