export const getTime = (e: React.ChangeEvent<HTMLInputElement>, date: string) => {
  const result = new Date(date);
  const time = e.target.value?.split(':');
  if (!!time && !!e.target.value && !e.target.value.includes('_')) {
    result.setHours(Number(time[0]), Number(time[1]), Number(time[2]));
    return result;
  }
  return null;
};
