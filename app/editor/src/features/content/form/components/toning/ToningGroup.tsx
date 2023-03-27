import { useFormikContext } from 'formik';
import React from 'react';
import { useLookupOptions } from 'store/hooks';
import { Col, Error, IContentModel, Row } from 'tno-core';

import * as styled from './styled';

export interface IToningGroupProps {
  /** The name of the field. */
  fieldName: keyof IContentModel;
}

/**
 * Provides a component form input for toning values.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ToningGroup: React.FC<IToningGroupProps> = ({ fieldName }) => {
  const [{ tonePools }] = useLookupOptions();
  const { values, setFieldValue, touched, errors } = useFormikContext<IContentModel>();

  const toningOptions = [-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5];
  const defaultTonePool = tonePools.find((t) => t.name === 'Default');
  const value = values.tonePools?.length ? values.tonePools[0].value : undefined;

  const [active, setActive] = React.useState(value);

  React.useEffect(() => {
    if (active !== value) {
      setActive(value);
    }
  }, [active, value]);

  const determineIndicator = (option: number) => {
    if (option === 5) {
      return (
        <img alt={option.toString()} src={`${process.env.PUBLIC_URL}/assets/face-grin-wide.svg`} />
      );
    } else if (option === 0) {
      return <img alt={option.toString()} src={`${process.env.PUBLIC_URL}/assets/face-meh.svg`} />;
    } else if (option === -5) {
      return (
        <img alt={option.toString()} src={`${process.env.PUBLIC_URL}/assets/face-frown-open.svg`} />
      );
    } else {
      return <span className="blank">&nbsp;</span>;
    }
  };

  return (
    <styled.ToningGroup className="multi-group">
      <label>Toning</label>
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
                  fieldName,
                  !!defaultTonePool ? [{ ...defaultTonePool, value: option }] : [],
                );
              }}
            >
              {option}
            </button>
          </Col>
        ))}
      </Row>
      <Error error={!!fieldName && touched[fieldName] ? errors[fieldName] : ''} />
    </styled.ToningGroup>
  );
};
