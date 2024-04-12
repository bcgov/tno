import React, { InputHTMLAttributes } from 'react';
import { FaClock } from 'react-icons/fa6';
import { Col, Error, Row, Show } from 'tno-core';

import * as styled from './styled';

export interface ITimeInputProps
  extends Omit<InputHTMLAttributes<HTMLInputElement>, 'children' | 'dangerouslySetInnerHTML'> {
  label?: string;
  /**
   * Display errors.
   */
  error?: string;
}

/** Component that will enforce the HH:MM:SS time format */
export const TimeInput: React.FC<ITimeInputProps> = ({ label, error, ...rest }) => {
  const [showMenuOptions, setShowMenuOptions] = React.useState(false);
  const [hours, setHours] = React.useState('');
  const [minutes, setMinutes] = React.useState('');
  const [seconds, setSeconds] = React.useState('');
  const [value, setValue] = React.useState('');
  const menuRef = React.useRef<HTMLDivElement>(null);
  const hourOptions = Array.from({ length: 24 }, (_, i) => String(i).padStart(2, '0'));
  const minuteSecondOptions = Array.from({ length: 60 }, (_, i) => String(i).padStart(2, '0'));

  React.useEffect(() => {
    const incomingValue = String(rest.value);
    // update if no user input yet and external value is differnt (navigating through list)
    if (!value || incomingValue !== value) {
      const isValidTimeFormat = /^([0-1]?[0-9]|2[0-3]):([0-5][0-9]):([0-5][0-9])$/.test(
        incomingValue,
      );
      if (isValidTimeFormat) {
        setValue(incomingValue);
      }
    }
    // only update if value changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rest.value]);

  React.useEffect(() => {
    if (hours || minutes || seconds) {
      // Check if any of these are non-empty
      setValue(`${hours}:${minutes}:${seconds}`);
    } else {
      !rest.value && setValue(''); // Set to empty to avoid "::"
    }
    // only update when hours, minutes, or seconds change
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hours, minutes, seconds]);

  // keep value in sync with hours, minutes, and seconds
  React.useEffect(() => {
    if (value.length === 8) {
      const [hour, minute, second] = value.split(':');
      setHours(hour);
      setMinutes(minute);
      setSeconds(second);
    }
  }, [value]);

  // Click outside handler close menu
  const handleClickOutside = (event: any) => {
    if (menuRef.current && !menuRef.current.contains(event.target)) {
      setShowMenuOptions(false);
    }
  };

  React.useEffect(() => {
    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  return (
    <styled.TimeInput {...rest}>
      <Show visible={!!label}>
        <label className={rest.required ? 'required' : ''}>{label}</label>
      </Show>
      <div className="input-area">
        <input
          type="text"
          value={value}
          className="masked-input"
          onChange={(e) => {
            const input = e.target.value;
            const originalInput = value;
            let numericInput = input.replace(/[^0-9]/g, ''); // do not allow any non-numeric characters

            let formattedTime = numericInput;
            if (numericInput.length > 6) {
              numericInput = numericInput.slice(0, 6); // Ensure max length of 6 digits
            }
            if (numericInput.length > 4) {
              formattedTime = `${numericInput.slice(0, 2)}:${numericInput.slice(
                2,
                4,
              )}:${numericInput.slice(4)}`;
            } else if (numericInput.length > 2) {
              formattedTime = `${numericInput.slice(0, 2)}:${numericInput.slice(2)}`;
            }

            // this fixes backspace issue on first character
            if (input.length < originalInput.length) {
              setValue(input);
            } else {
              // enforce HH:MM:SS format with only valid numbers (eg 32:99:99 is invalid)
              const regex = /^([0-1]?[0-9]|2[0-3])?(:([0-5]?[0-9])(:([0-5]?[0-9])?)?)?$/;
              if (regex.test(formattedTime)) {
                setValue(formattedTime);
              }
            }
          }}
          placeholder="hh:mm:ss"
          maxLength={8}
        />
        {<FaClock onClick={() => setShowMenuOptions(true)} />}
      </div>
      {showMenuOptions && (
        <>
          <div className="arrow-up"></div>
          <Row className="time-menu" ref={menuRef}>
            <Col className="select-col">
              <div className="col-header">Hour</div>
              <div className="hour-menu">
                {hourOptions.map((hour) => (
                  <div
                    className={`time-opt ${hour === hours ? 'selected' : ''}`}
                    onClick={() => {
                      setHours(String(hour));
                    }}
                    key={`${hour.toString()}-hour`}
                  >
                    {hour}
                  </div>
                ))}
              </div>
            </Col>
            <Col className="select-col">
              <div className="col-header">Minute</div>
              <div className="minute-menu">
                {minuteSecondOptions.map((minute) => (
                  <div
                    className={`time-opt ${minute === minutes ? 'selected' : ''}`}
                    onClick={() => {
                      setMinutes(String(minute));
                    }}
                    key={`${minute.toString()}-minute`}
                  >
                    {minute}
                  </div>
                ))}
              </div>
            </Col>
            <Col className="select-col">
              <div className="col-header">Seconds</div>
              <div className="second-menu">
                {minuteSecondOptions.map((second) => (
                  <div
                    className={`time-opt ${second === seconds ? 'selected' : ''}`}
                    onClick={() => {
                      setSeconds(String(second));
                    }}
                    key={`${second.toString()}-second`}
                  >
                    {second}
                  </div>
                ))}
              </div>
            </Col>
          </Row>
        </>
      )}
      <Error error={error} />
    </styled.TimeInput>
  );
};
