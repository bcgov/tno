import {
  type IActionModel,
  type ICacheModel,
  type IContributorModel,
  type IDataLocationModel,
  type IHolidayModel,
  type IIngestTypeModel,
  type ILicenseModel,
  type IMediaTypeModel,
  type IMetricModel,
  type IMinisterModel,
  type IOrganizationModel,
  type IRoleModel,
  type ISeriesModel,
  type ISettingModel,
  type ISourceActionModel,
  type ISourceModel,
  type ITagModel,
  type ITonePoolModel,
  type ITopicModel,
  type ITopicScoreRuleModel,
  type IUserModel,
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
