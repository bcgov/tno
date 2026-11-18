/** Simple function used to determine if it is the weekend or a weekday. Used to calculate what timerange of commentary should be shown. */
export const isWeekday = () => {
  const d = new Date();
  const day = d.getDay();
  return day !== 0 && day !== 6;
};
