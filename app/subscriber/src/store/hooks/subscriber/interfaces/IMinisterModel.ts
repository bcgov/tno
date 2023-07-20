import { IOrganizationModel, ISortableModel } from 'tno-core';

export interface IMinisterModel extends ISortableModel<number> {
  aliases: string;
  description: string;
  organizationId?: number;
  organization?: IOrganizationModel;
  position: string;
}
