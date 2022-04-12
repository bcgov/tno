export const formatTime = (date: string) => {
  const converted = new Date(date);
  return !!converted.getTime() ? converted.getTime() : '';
};
