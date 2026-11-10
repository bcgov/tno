import { ConnectionTypeName, OptionItem } from 'tno-core';

export const connectionTypeOptions = Object.values(ConnectionTypeName).map(
  (v) => new OptionItem(v, v),
);
