import { IValidationError } from '.';

export interface IResponseError {
  type: string;
  title: string;
  status: number;
  traceId?: string;
  errors: IValidationError[];
}
