import { ISortableModel } from '.';
import { ConnectionTypeName } from '..';

export interface IConnectionModel extends ISortableModel<number> {
  connectionType: ConnectionTypeName;
  configuration?: any;
  isReadOnly: boolean;
}
