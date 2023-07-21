import { IOrganizationModel, ISortableModel } from '.';

export interface IMinisterModel extends ISortableModel<number> {
  aliases: string;
  position: string;
  organizationId?: number;
  organization?: IOrganizationModel;
}
