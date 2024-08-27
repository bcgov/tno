export const selectColorForDataset = (index: number, colors: string[]) => {
  if (index < 0) return colors;
  if (index < colors.length) return colors[index];
  // Loop through the colours based on the index position.
  let position = index < colors.length ? index : index % colors.length;
  return [colors[position]];
};
