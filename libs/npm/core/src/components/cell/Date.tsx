import moment from 'moment';
import React from 'react';

interface IDateProps {
  value?: Date | string | moment.Moment;
}

export const Date: React.FC<IDateProps> = ({ value }) => {
  if (!!value) {
    const created = moment(value);
    const text = created.isValid() ? created.format('MM/DD/YYYY') : '';
    return <>{text}</>;
  }
  return null;
};
