import styled from 'styled-components';

import { IRowProps } from '..';

export const Row = styled.div<IRowProps>`
  display: flex;
  flex-direction: ${(props) => props.direction};
  flex: ${(props) => props.flex};
  flex-wrap: ${(props) => (props.nowrap ? 'no-wrap' : props.wrap ?? 'wrap')};
  flex-grow: ${(props) => props.grow};
  flex-shrink: ${(props) => props.shrink};
  flex-basis: ${(props) => props.basis};
  align-items: ${(props) => props.alignItems};
  align-content: ${(props) => props.alignContent};
  align-self: ${(props) => props.alignSelf};
  gap: ${(props) => props.gap};
  row-gap: ${(props) => props.rowGap};
  column-gap: ${(props) => props.colGap};
`;
