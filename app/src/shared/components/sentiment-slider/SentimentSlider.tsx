import 'rc-slider/assets/index.css';

import Slider from 'rc-slider';
import { FaX } from 'react-icons/fa6';

import { sentimentOptions } from './constants';
import * as styled from './styled';

export interface ISentimentSliderProps {
  /** The label to display above the slider */
  label?: string;
  /** Event fires when value changes. */
  onChange: (value: number | number[] | undefined) => void;
  /** The current value(s). */
  value?: number | number[];
  /** The default value when slider is cleared. */
  defaultValue?: number | number[];
  /** Class name of component (defaults to 'slider'). */
  className?: string;
  /** Whether the slider is clearable. */
  isClearable?: boolean;
  /** Whether the slider is disabled. */
  disabled?: boolean;
}

/**
 * Provides a sentiment slider that enables selecting a range.
 * @param param0 Component properties.
 * @returns A component.
 */
export const SentimentSlider: React.FC<ISentimentSliderProps> = ({
  label,
  value,
  defaultValue,
  onChange,
  className,
  isClearable = true,
  disabled,
}) => {
  return (
    <styled.SentimentSlider className={`frm-in${className ? ` ${className}` : ' slider'}`}>
      {label && <label>{label}</label>}
      <div>
        <Slider
          marks={sentimentOptions}
          range
          min={-5}
          max={5}
          value={value ?? defaultValue}
          onChange={onChange}
          disabled={disabled}
        />
        {isClearable && <FaX onClick={() => onChange?.(defaultValue)} />}
      </div>
    </styled.SentimentSlider>
  );
};
