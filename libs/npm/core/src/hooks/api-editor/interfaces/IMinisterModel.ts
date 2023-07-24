import { IOrganizationModel, ISortableModel } from '.';

export interface IMinisterModel extends ISortableModel<number> {
  aliases: string;
  organizationId?: number;
  organization?: IOrganizationModel;
  position: string;
}
