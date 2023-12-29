import { IUserLocationModel } from './IUserLocationModel';

export interface IUserPreferencesModel {
  locations?: IUserLocationModel[];
  [key: string]: any;
}
