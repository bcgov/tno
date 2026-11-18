import { IActionModel } from '.';

/** model used for sending and appending actions to the api */
export interface IContentActionModel extends IActionModel {
  value: string;
}
