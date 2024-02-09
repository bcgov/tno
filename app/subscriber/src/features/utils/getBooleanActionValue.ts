export const getBooleanActionValue = (actionId: string) => {
  const action = {
    id: +actionId,
    value: String(true),
    valueType: 'Boolean',
  };
  return action;
};
