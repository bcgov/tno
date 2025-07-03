import { IMinisterModel, ISortableModel, IUserModel } from '.';

export interface IOrganizationModel extends ISortableModel<number> {
  parentId?: number;
  parent?: IOrganizationModel;
  children: IOrganizationModel[];
  users: IUserModel[];
  ministers: IMinisterModel[];
  name: string;
}
