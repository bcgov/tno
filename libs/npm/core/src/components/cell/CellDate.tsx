import moment from 'moment';
import React from 'react';

interface ICellDateProps {
  /** The date */
  value?: Date | string | moment.Moment;
  /** The format, defaults to 'MM/DD/YYYY' */
  format?: string;
}

/**
 * Returns a formatted date value.
 * @param param0 Component properties
 * @returns The data formatted as a string.
 */
export const CellDate: React.FC<ICellDateProps> = ({ value, format = 'MM/DD/YYYY HH:mm:ss' }) => {
  if (value !== undefined) {
    const created = moment(value);
    const text = created.isValid() ? created.format(format) : '';
    return <>{text}</>;
  }
  return null;
};
