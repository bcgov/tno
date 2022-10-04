/** Used to only display lookup options that are enabled within dropdowns */
export const filterEnabled = (data: any[]) => {
  return data.filter((d) => d.isEnabled);
};
