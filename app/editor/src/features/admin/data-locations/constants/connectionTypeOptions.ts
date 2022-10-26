import { OptionItem } from 'components/form';
import { ConnectionTypeName } from 'hooks/api-editor';

export const connectionTypeOptions = Object.values(ConnectionTypeName).map(
  (v) => new OptionItem(v, v),
);
