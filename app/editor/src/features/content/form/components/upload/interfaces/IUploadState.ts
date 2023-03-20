import { FetchState } from './FetchState';

export interface IUploadState {
  name: string;
  status: FetchState;
  loaded: number;
  total: number;
}
