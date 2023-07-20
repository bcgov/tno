import { IOrganizationModel, ISortableModel } from '.';

export interface IMinisterModel extends ISortableModel<number> {
  aliases: string;
  organizationId?: number;
  aliases?: string;
  organization?: IOrganizationModel;
  position: string;
}
