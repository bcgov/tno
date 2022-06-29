import moment from 'moment';

/**
 * Attempts to format the value in the text input.
 * @param value The text input value.
 * @param format The format you want to apply.  Defaults to local.
 * @returns A formatted date value, or the original value provided.
 */
export const formatDate = (
  value: string | number | readonly string[] | undefined,
  format?: string,
) => {
  if (!!value && typeof value === 'string') {
    var date = moment(value);
    if (date.isValid())
      return !!format ? moment(value).format(format) : moment(value).toLocaleString();
  }
  return value;
};
