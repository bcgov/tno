import { InputHTMLAttributes } from 'react';
import MaskedInput from 'react-text-mask';
import { Show } from 'tno-core';

import * as styled from './styled';
export interface ITimeInputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
}

/** Component that will enforce the HH:MM:SS time format */
export const TimeInput: React.FC<ITimeInputProps> = ({ label, ...rest }) => {
  const formatTime = (value: string) => {
    const chars = value.split('');
    const hours = [/[0-2]/, chars[0] === '2' ? /[0-3]/ : /[0-9]/] as any;

    const minutes = [/[0-5]/, /[0-9]/];
    const seconds = [/[0-5]/, /[0-9]/];

    return hours.concat(':').concat(minutes).concat(':').concat(seconds) as any;
  };
  return (
    <styled.TimeInput>
      <Show visible={!!label}>
        <label className={rest.required ? 'required' : ''}>{label}</label>
      </Show>
      <MaskedInput className="masked-input" {...rest} mask={formatTime} />
    </styled.TimeInput>
  );
};
