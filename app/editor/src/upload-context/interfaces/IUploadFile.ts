import { FetchState } from './FetchState';

export interface IUploadFile {
  name: string;
  status: FetchState;
  loaded: number;
  total: number;
}
