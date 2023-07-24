import { IOrganizationModel, ISortableModel } from 'tno-core';

export interface IMinisterModel extends ISortableModel<number> {
  aliases: string;
  organizationId?: number;
  organization?: IOrganizationModel;
  position: string;
}
