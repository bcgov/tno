import { IMediaTypeSettingsModel, ISortableModel } from '.';

export interface IMediaTypeModel extends ISortableModel<number> {
  autoTranscribe: boolean;
  settings: IMediaTypeSettingsModel;
}
