import { ITransferFilter } from './ITransferFilter';
import { ITransferFolder } from './ITransferFolder';
import { ITransferObject } from './ITransferObject';

export interface ITransferReportSection extends ITransferObject {
  filterId?: number;
  filter?: ITransferFilter;
  folderId?: number;
  folder?: ITransferFolder;
  linkedReportId?: number;
}
