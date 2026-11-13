import { maskitoTimeOptionsGenerator, maskitoWithPlaceholder } from '@maskito/kit';
import { useMaskito } from '@maskito/react';
import React, { InputHTMLAttributes } from 'react';

import { Error } from '../../form';
import { Show } from '../../show';
import * as styled from './styled';

export interface ITimeInputProps extends Omit<
  InputHTMLAttributes<HTMLInputElement>,
  'children' | 'dangerouslySetInnerHTML'
> {
  label?: string;
  /**
   * Display errors.
   */
  error?: string;
}

// HH:MM:SS with hours 00-23 and minutes/seconds 00-59. While typing, the unfilled positions show
// as '_' (e.g. '1_:__:__'), as before; consumers turn them into zeros on blur.
const time = maskitoTimeOptionsGenerator({ mode: 'HH:MM:SS' });
const { plugins, ...guide } = maskitoWithPlaceholder('__:__:__');
const timeMask = {
  ...time,
  plugins: [...plugins, ...(time.plugins ?? [])],
  preprocessors: [...guide.preprocessors, ...(time.preprocessors ?? [])],
  postprocessors: [...(time.postprocessors ?? []), ...guide.postprocessors],
};

/** Component that will enforce the HH:MM:SS time format */
export const TimeInput: React.FC<ITimeInputProps> = ({ label, error, value, ...rest }) => {
  const maskRef = useMaskito({ options: timeMask });
  const inputRef = React.useRef<HTMLInputElement | null>(null);

  // The input owns its text while the user types (some consumers only read it on blur) and is
  // updated whenever the value passed in changes, as the previous masked input did.
  React.useEffect(() => {
    if (inputRef.current) inputRef.current.value = value === undefined ? '' : `${value}`;
  }, [value]);

  return (
    <styled.TimeInput {...rest}>
      <Show visible={!!label}>
        <label className={rest.required ? 'required' : ''}>{label}</label>
      </Show>
      <input
        ref={(element) => {
          inputRef.current = element;
          maskRef(element);
        }}
        type="text"
        role={!!error ? 'alert' : 'none'}
        className="masked-input"
        defaultValue={value}
        {...rest}
      />
      <Error error={error} />
    </styled.TimeInput>
  );
};
