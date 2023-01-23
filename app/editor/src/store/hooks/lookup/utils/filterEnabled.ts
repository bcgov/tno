/** Used to only display lookup options that are enabled within dropdowns */
export const filterEnabled = (data: any[], currentSelected: any = null) => {
  return data.filter((d) => d.isEnabled || (!!currentSelected && d.value === currentSelected));
};
