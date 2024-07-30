import { IMinisterModel } from 'store/hooks/subscriber/interfaces/IMinisterModel';
import {
  IActionModel,
  ICacheModel,
  IContributorModel,
  IDataLocationModel,
  IHolidayModel,
  IIngestTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  IMetricModel,
  IRoleModel,
  ISeriesModel,
  ISettingModel,
  ISourceActionModel,
  ISourceModel,
  ISystemMessageModel,
  ITagModel,
  ITonePoolModel,
  ITopicModel,
  ITopicScoreRuleModel,
  IUserModel,
} from 'tno-core';

export interface ILookupState {
  isReady: boolean;
  cache: ICacheModel[];
  actions: IActionModel[];
  topics: ITopicModel[];
  ministers: IMinisterModel[];
  mediaTypes: IMediaTypeModel[];
  licenses: ILicenseModel[];
  series: ISeriesModel[];
  contributors: IContributorModel[];
  sources: ISourceModel[];
  sourceActions: ISourceActionModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  holidays: IHolidayModel[];
  ingestTypes: IIngestTypeModel[];
  roles: IRoleModel[];
  metrics: IMetricModel[];
  users: IUserModel[];
  dataLocations: IDataLocationModel[];
  systemMessages: ISystemMessageModel[];
  settings: ISettingModel[];
  rules: ITopicScoreRuleModel[];
  frontPageImagesMediaTypeId?: number;
}
