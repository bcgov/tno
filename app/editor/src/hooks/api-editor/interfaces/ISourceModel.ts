import { ILicenseModel, IMetricModel, ISortableModel, ISourceActionModel, IUserModel } from '.';

export interface ISourceModel extends ISortableModel<number> {
  code: string;
  shortName: string;
  licenseId: number;
  license?: ILicenseModel;
  ownerId?: number;
  owner?: IUserModel;
  autoTranscribe: boolean;
  disableTranscribe: boolean;
  actions: ISourceActionModel[];
  metrics: IMetricModel[];
}
