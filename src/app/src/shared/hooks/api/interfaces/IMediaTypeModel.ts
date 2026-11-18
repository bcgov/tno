import { IMediaTypeSettingsModel, ISortableModel } from '.';
import { ListOptionName } from '../constants';

export interface IMediaTypeModel extends ISortableModel<number> {
  autoTranscribe: boolean;
  listOption: ListOptionName;
  settings: IMediaTypeSettingsModel;
}
