import React from 'react';
import { Col, IContentTonePoolModel, Row } from 'tno-core';

import * as styled from './styled';

const toningOptions = [-5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5];

interface IToneSelectorProps {
  label?: string;
  myTonePool: IContentTonePoolModel;
  onSelect: (value: number) => void;
}

export const ToneSelector: React.FC<IToneSelectorProps> = ({
  label = 'Tone',
  onSelect,
  myTonePool,
}) => {
  const facePositive = `${process.env.PUBLIC_URL}/assets/reports/face-positive@2x.png`;
  const faceNeutral = `${process.env.PUBLIC_URL}/assets/reports/face-neutral@2x.png`;
  const faceNegative = `${process.env.PUBLIC_URL}/assets/reports/face-negative@2x.png`;
  const value = myTonePool?.value;
  const [active, setActive] = React.useState<number | undefined>(value);

  const determineIndicator = (option: number) => {
    if (option === 5) {
      return <img alt={option.toString()} src={facePositive} />;
    } else if (option === 0) {
      return <img alt={option.toString()} src={faceNeutral} />;
    } else if (option === -5) {
      return <img alt={option.toString()} src={faceNegative} />;
    } else {
      return <span className="blank">&nbsp;</span>;
    }
  };

  const handleToneSelect = (option: number) => {
    setActive(option);
    onSelect(option);
  };

  return (
    <styled.ToneSelector>
      <label>{label}</label>
      <Row>
        {toningOptions.map((option) => (
          <Col key={option}>
            {determineIndicator(option)}
            <button
              className={active === option ? 'active' : ''}
              type="button"
              onClick={() => handleToneSelect(option)}
            >
              {option}
            </button>
          </Col>
        ))}
      </Row>
    </styled.ToneSelector>
  );
};
