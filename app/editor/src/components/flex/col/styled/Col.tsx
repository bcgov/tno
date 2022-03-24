import styled from 'styled-components';

import { IColProps } from '..';

export const Col = styled.div<IColProps>`
  display: flex;
  flex-direction: ${(props) => props.direction};
  flex: ${(props) => props.flex};
  flex-wrap: ${(props) => props.wrap};
  flex-grow: ${(props) => props.grow};
  flex-shrink: ${(props) => props.shrink};
  flex-basis: ${(props) => props.basis};
  align-items: ${(props) => props.alignItems};
  align-content: ${(props) => props.alignContent};
  gap: ${(props) => props.gap};
  row-gap: ${(props) => props.rowGap};
  column-gap: ${(props) => props.colGap};

  & > button {
    align-self: flex-start;
  }
`;
