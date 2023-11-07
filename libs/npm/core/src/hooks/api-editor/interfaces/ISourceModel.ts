import { ILicenseModel, IMetricModel, ISortableModel, ISourceActionModel, IUserModel } from '.';

export interface ISourceModel extends ISortableModel<number> {
  code: string;
  shortName: string;
  licenseId: number;
  license?: ILicenseModel;
  ownerId?: number;
  productId?: number;
  productSearchGroupId?: number;
  owner?: IUserModel;
  autoTranscribe: boolean;
  disableTranscribe: boolean;
  useInTopics: boolean;
  configuration: any;
  actions: ISourceActionModel[];
  metrics: IMetricModel[];
}
