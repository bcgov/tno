export const selectSentimentColorForValue = (value: number | string, colors: string[]) => {
  return (ctx: any, options: any) => {
    const index = ctx.dataIndex;
    const value = ctx.dataset.data[index];
    if (typeof value === 'number')
      return value === null ? null : value > 0 ? colors[0] : value === 0 ? colors[1] : colors[2];
  };
};
