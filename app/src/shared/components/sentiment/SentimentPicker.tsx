import React from 'react';

import FaceFrownOpen from '../../assets/face-frown-open.svg';
import FaceGrinWide from '../../assets/face-grin-wide.svg';
import FaceMeh from '../../assets/face-meh.svg';
import { Col, Error, Row } from '../../components';
import * as styled from './styled';

const toningOptions = [-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5];

export interface ISentimentPickerProps<T>
  extends Omit<React.HTMLAttributes<HTMLDivElement>, 'onChange'> {
  /** The name of the field. */
  name: string | keyof T;
  /** The label of the field. */
  label?: string;
  /** Whether component is disabled. */
  disabled?: boolean;
  /** Whether this field is required. */
  required?: boolean;
  value?: number;
  error?: string;
  onChange?: (value: number) => void;
}

/**
 * Provides a component form input for toning values.
 * @param param0 Component properties.
 * @returns Component.
 */
export const SentimentPicker = <T extends object>({
  name,
  label = 'Sentiment',
  value: initValue,
  disabled,
  error,
  onChange,
  required,
  className,
  ...rest
}: ISentimentPickerProps<T>) => {
  const [active, setActive] = React.useState(initValue);

  React.useEffect(() => {
    setActive(initValue);
  }, [initValue]);

  const determineIndicator = (option: number) => {
    if (option === 5) {
      return <img alt={option.toString()} src={FaceGrinWide} />;
    } else if (option === 0) {
      return <img alt={option.toString()} src={FaceMeh} />;
    } else if (option === -5) {
      return <img alt={option.toString()} src={FaceFrownOpen} />;
    } else {
      return <span className="blank">&nbsp;</span>;
    }
  };

  return (
    <styled.SentimentPicker
      className={`frm-in multi-group${className ? ` ${className}` : ''}`}
      required={required}
      {...rest}
    >
      {label && <label>{label}</label>}
      <Row>
        {' '}
        {toningOptions.map((option) => (
          <Col key={option}>
            {determineIndicator(option)}
            <button
              className={active === option ? 'active' : ''}
              type="button"
              key={option}
              disabled={disabled}
              onClick={() => {
                setActive(option);
                onChange?.(option);
              }}
            >
              {option}
            </button>
          </Col>
        ))}
      </Row>
      <Error error={error?.toString()} />
    </styled.SentimentPicker>
  );
};
