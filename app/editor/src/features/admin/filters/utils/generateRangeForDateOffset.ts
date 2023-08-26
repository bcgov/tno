export const generateRangeForDateOffset = (field: string, value: number) => {
  return [
    {
      range: {
        [field]: {
          gte: `now${value > 0 ? `+${value}d/d` : value < 0 ? `${value}d/d` : '/d'}`,
          time_zone: 'US/Pacific',
        },
      },
    },
  ];
};
