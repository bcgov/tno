import {
  IActionModel,
  ICacheModel,
  IContributorModel,
  IDataLocationModel,
  IHolidayModel,
  IIngestTypeModel,
  ILicenseModel,
  ILLMModel,
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
  IUserModel,
} from 'tno-core';

import { IMinisterModel } from '@/subscriber/store/hooks/subscriber/interfaces/IMinisterModel';

export interface ILookupState {
  isReady: boolean;
  cache: ICacheModel[];
  actions: IActionModel[];
  topics: ITopicModel[];
  llms: ILLMModel[];
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
  frontPageImagesMediaTypeId?: number;
}
