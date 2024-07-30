import {
  IActionModel,
  IContributorModel,
  IDataLocationModel,
  IHolidayModel,
  IIngestTypeModel,
  ILicenseModel,
  IMediaTypeModel,
  IMetricModel,
  IMinisterModel,
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
} from '.';

export interface ILookupModel {
  actions: IActionModel[];
  topics: ITopicModel[];
  rules: ITopicScoreRuleModel[];
  mediaTypes: IMediaTypeModel[];
  sources: ISourceModel[];
  licenses: ILicenseModel[];
  ingestTypes: IIngestTypeModel[];
  roles: IRoleModel[];
  series: ISeriesModel[];
  contributors: IContributorModel[];
  sourceActions: ISourceActionModel[];
  metrics: IMetricModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
  dataLocations: IDataLocationModel[];
  settings: ISettingModel[];
  holidays: IHolidayModel[];
  ministers: IMinisterModel[];
  systemMessages: ISystemMessageModel[];
}
