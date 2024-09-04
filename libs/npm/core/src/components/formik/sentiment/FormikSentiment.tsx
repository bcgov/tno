import { getIn, useFormikContext } from 'formik';
import React from 'react';

import FaceFrownOpen from '../../../assets/face-frown-open.svg';
import FaceGrinWide from '../../../assets/face-grin-wide.svg';
import FaceMeh from '../../../assets/face-meh.svg';
import { Col, Error, Row } from '../../../components';
import { ITonePoolModel } from '../../../hooks';
import * as styled from './styled';

const toningOptions = [-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5];

export interface IFormikSentimentProps<T> {
  /** The name of the field. */
  name: string | keyof T;
  /** The label of the field. */
  label?: string;
  /** Sentiment options. */
  options: ITonePoolModel[];
  /** The name of the default tone pool */
  defaultTonePoolName?: string;
  /** The id of the default tone pool */
  defaultTonePoolId?: number;
  /** Whether this field is required. */
  required?: boolean;
}

/**
 * Provides a component form input for toning values.
 * @param param0 Component properties.
 * @returns Component.
 */
export const FormikSentiment = <T extends object>({
  name,
  label = 'Sentiment',
  options,
  defaultTonePoolId,
  defaultTonePoolName = 'Default',
  ...rest
}: IFormikSentimentProps<T>) => {
  const { values, setFieldValue, touched, errors } = useFormikContext<T>();

  const SentimentProps: IFormikSentimentProps<T> = {
    name,
    label,
    options,
    defaultTonePoolId,
    defaultTonePoolName,
    ...rest,
  };
  const defaultTonePool = options.find(
    (t) => t.id === defaultTonePoolId || t.name === defaultTonePoolName,
  );
  const tonePools = getIn(values, name.toString());
  const value =
    tonePools && Array.isArray(tonePools) && tonePools.length ? tonePools[0].value : undefined;

  const [active, setActive] = React.useState(value);

  const errorMessage = getIn(errors, name.toString());
  const error = errorMessage && getIn(touched, name.toString()) && errorMessage;

  React.useEffect(() => {
    if (active !== value) {
      setActive(value);
    }
  }, [active, value]);

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
    <styled.FormikSentiment className="frm-in multi-group" {...SentimentProps}>
      <label>{label}</label>
      <Row>
        {' '}
        {toningOptions.map((option) => (
          <Col key={option}>
            {determineIndicator(option)}
            <button
              className={active === option ? 'active' : ''}
              type="button"
              key={option}
              onClick={() => {
                setActive(option);
                setFieldValue(
                  name.toString(),
                  !!defaultTonePool ? [{ ...defaultTonePool, value: option }] : [],
                  true,
                );
              }}
            >
              {option}
            </button>
          </Col>
        ))}
      </Row>
      <Error error={error?.toString()} />
    </styled.FormikSentiment>
  );
};
