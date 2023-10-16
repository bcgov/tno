import { IProductSettingsModel, ISortableModel } from '.';

export interface IProductModel extends ISortableModel<number> {
  autoTranscribe: boolean;
  settings: IProductSettingsModel;
}
