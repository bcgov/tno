import { ConnectionTypeName, IConnectionModel } from 'tno-core';

export const defaultConnection: IConnectionModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 0,
  connectionType: ConnectionTypeName.LocalVolume,
  isReadOnly: false,
};
