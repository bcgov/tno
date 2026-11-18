import { IValidationErrorModel } from '.';

export interface IResponseErrorModel {
  type: string;
  title: string;
  status: number;
  traceId?: string;
  errors: IValidationErrorModel[];
}
