import {
  ILicenseModel,
  IMediaTypeModel,
  IMetricModel,
  ISortableModel,
  ISourceActionModel,
  IUserModel,
} from '.';

export interface ISourceModel extends ISortableModel<number> {
  code: string;
  shortName: string;
  licenseId: number;
  license?: ILicenseModel;
  ownerId?: number;
  mediaTypeId?: number;
  mediaType?: IMediaTypeModel;
  mediaTypeSearchMappings: IMediaTypeModel[];
  owner?: IUserModel;
  autoTranscribe: boolean;
  disableTranscribe: boolean;
  useInTopics: boolean;
  configuration: any;
  actions: ISourceActionModel[];
  metrics: IMetricModel[];
}
