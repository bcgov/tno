import { ITransferFilter } from './ITransferFilter';
import { ITransferObject } from './ITransferObject';

export interface ITransferFolder extends ITransferObject {
  filterId?: number;
  filter?: ITransferFilter;
}
