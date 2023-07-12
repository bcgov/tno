import { IOrganizationModel, ISortableModel } from '.';

export interface IMinisterModel extends ISortableModel<number> {
  position: string;
  organizationId?: number;
  organization?: IOrganizationModel;
}
