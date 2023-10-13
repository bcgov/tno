import { generateQueryForAction } from './generateQueryForAction';

export const generateQueryForActions = (
  actions: {
    id: number;
    value: string;
    valueType?: string;
  }[],
) => {
  return actions.map((a) => generateQueryForAction(a.id, a.value, a.valueType));
};
