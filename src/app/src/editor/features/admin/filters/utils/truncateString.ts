export const truncateString = (str: string | undefined, num: number = 20) => {
  if (!str) return '';
  if (str.length > num) {
    return str.slice(0, num) + '...';
  }
  return str;
};
