import styled from 'styled-components';

import { IColProps } from '..';

export const Col = styled.div.attrs<IColProps>(({ className }) => ({
  className,
}))<IColProps>`
  display: flex;
  flex-wrap: ${(props) => (props.nowrap ? 'nowrap' : props.wrap ?? 'wrap')};
  ${(props) => props.direction !== undefined && `flex-direction: ${props.direction};`}
  ${(props) => props.grow !== undefined && `flex-grow: ${props.grow};`}
  ${(props) => props.shrink !== undefined && `flex-shrink: ${props.shrink};`}
  ${(props) => props.basis !== undefined && `flex-basis: ${props.basis};`}
  ${(props) => props.flex !== undefined && `flex: ${props.flex};`}
  ${(props) => props.justifyContent !== undefined && `justify-content: ${props.justifyContent};`}
  ${(props) => props.justifyItems !== undefined && `justify-items: ${props.justifyItems};`}
  ${(props) => props.alignItems !== undefined && `align-items: ${props.alignItems};`}
  ${(props) => props.alignContent !== undefined && `align-content: ${props.alignContent};`}
  ${(props) => props.alignSelf !== undefined && `align-self: ${props.alignSelf};`}
  ${(props) => props.rowGap !== undefined && `row-gap: ${props.rowGap};`}
  ${(props) => props.colGap !== undefined && `column-gap: ${props.colGap};`}
  ${(props) => props.gap !== undefined && `gap: ${props.gap};`}
`;
