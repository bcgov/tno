import { ConnectionTypeName, OptionItem } from '@/shared';

export const connectionTypeOptions = Object.values(ConnectionTypeName).map(
  (v) => new OptionItem(v, v),
);
