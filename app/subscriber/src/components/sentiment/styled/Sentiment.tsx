import styled from 'styled-components';

import { ISentimentProps } from '../Sentiment';

export const Sentiment = styled.div<ISentimentProps>`
  display: flex;
  flex-direction: row;
  gap: 0.5rem;
  color: ${(props) => {
    if (props.value !== undefined) {
      if (props.value < 0) return '#DC3545';
      else if (props.value === 0) return '#FFC107';
      else if (props.value > 0) return '#20C997';
    }
    return '#E0E0E0';
  }};
  svg {
    height: 1.25em;
    width: 1.25em;
    margin-left: 0.5em;
  }
`;
