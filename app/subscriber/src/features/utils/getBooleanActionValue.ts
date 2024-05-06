export const getBooleanActionValue = (actionId: number) => {
  const action = {
    id: actionId,
    value: String(true),
    valueType: 'Boolean',
  };
  return action;
};
