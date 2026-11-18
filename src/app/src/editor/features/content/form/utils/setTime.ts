export const setTime = (publishedOn: string, time?: string) => {
  const date = new Date(publishedOn);
  const hours = time?.split(':');
  if (!!hours && !!time && !time.includes('_')) {
    date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
    return date;
  }
  return null;
};
