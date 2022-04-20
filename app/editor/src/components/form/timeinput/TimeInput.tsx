import { InputHTMLAttributes } from 'react';

import * as styled from './styled';

/** Component that will enforce the HH:MM:SS time format */
export const TimeInput: React.FC<InputHTMLAttributes<HTMLInputElement>> = (props) => {
  const formatTime = (value: string) => {
    const chars = value.split('');
    const hours = [/[0-2]/, chars[0] === '2' ? /[0-3]/ : /[0-9]/] as any;

    const minutes = [/[0-5]/, /[0-9]/];
    const seconds = [/[0-5]/, /[0-9]/];

    return hours.concat(':').concat(minutes).concat(':').concat(seconds) as any;
  };
  return <styled.TimeInput {...props} mask={formatTime} />;
};
