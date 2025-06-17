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
  IMinisterModel,
  IOrganizationModel,
  IRoleModel,
  ISeriesModel,
  ISettingModel,
  ISourceActionModel,
  ISourceModel,
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
  rules: ITopicScoreRuleModel[];
  mediaTypes: IMediaTypeModel[];
  licenses: ILicenseModel[];
  ingestTypes: IIngestTypeModel[];
  roles: IRoleModel[];
  series: ISeriesModel[];
  contributors: IContributorModel[];
  sources: ISourceModel[];
  sourceActions: ISourceActionModel[];
  metrics: IMetricModel[];
  tags: ITagModel[];
  tonePools: ITonePoolModel[];
  users: IUserModel[];
  dataLocations: IDataLocationModel[];
  settings: ISettingModel[];
  holidays: IHolidayModel[];
  ministers: IMinisterModel[];
  organizations: IOrganizationModel[];
}
