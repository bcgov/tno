import { ITransferObject } from './ITransferObject';
import { ITransferReportSection } from './ITransferReportSection';

export interface ITransferReport extends ITransferObject {
  sections: ITransferReportSection[];
}
