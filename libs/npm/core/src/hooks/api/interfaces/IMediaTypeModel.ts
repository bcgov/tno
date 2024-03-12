import { ListOption } from '../constants';
import { IMediaTypeSettingsModel, ISortableModel } from '.';

export interface IMediaTypeModel extends ISortableModel<number> {
  autoTranscribe: boolean;
  listOption: ListOption;
  settings: IMediaTypeSettingsModel;
}
