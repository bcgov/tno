import { UserChangeTypeName } from '../constants';
import { IAuditColumnsModel } from './IAuditColumnsModel';
import { IUserModel } from './IUserModel';

export interface IUserUpdateHistoryModel extends IAuditColumnsModel {
  id: number;
  /** Foreign key to user */
  userId: number;
  /** User object */
  user?: IUserModel;

  ChangeType: UserChangeTypeName;
  value: string;
  dateOfChange: Date;
}
