import { IUserEmailModel } from './IUserEmailModel';
import { IUserLocationModel } from './IUserLocationModel';

export interface IUserPreferencesModel {
  locations?: IUserLocationModel[];
  addresses?: IUserEmailModel[];
  [key: string]: any;
}
