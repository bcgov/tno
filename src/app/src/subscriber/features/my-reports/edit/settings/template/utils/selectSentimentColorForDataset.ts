export const selectSentimentColorForDataset = (datasetName: number | string, colors: string[]) => {
  const _colors = colors.length >= 3 ? colors : ['green', 'gold', 'red'];

  if (typeof datasetName === 'number') {
    if (datasetName > 0) return [_colors[0]];
    if (datasetName === 0) return [_colors[1]];
    if (datasetName < 0) return [_colors[2]];
  }

  if (datasetName === 'Positive') return [_colors[0]];
  if (datasetName === 'Neutral') return [_colors[1]];
  if (datasetName === 'Negative') return [_colors[2]];
  return _colors;
};
