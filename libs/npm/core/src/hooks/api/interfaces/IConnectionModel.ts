import { ConnectionTypeName } from '..';
import { ISortableModel } from '.';

export interface IConnectionModel extends ISortableModel<number> {
  connectionType: ConnectionTypeName;
  configuration?: any;
  isReadOnly: boolean;
}
