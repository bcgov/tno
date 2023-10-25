export const generateRangeForDateOffset = (field: string, value: number) => {
  if (value === undefined || value === null || value === 3) return undefined;
  return {
    range: {
      [field]: {
        gte: `now${value > 0 ? `-${value}d/d` : value < 0 ? `${value}d/d` : '/d'}`,
        time_zone: 'US/Pacific',
      },
    },
  };
};
