import React from 'react';

import * as styled from './styled';

export interface IRowProps extends React.HTMLAttributes<HTMLDivElement> {
  width?: string;
  flex?: string;
  grow?: number;
  shrink?: number;
  basis?: string | 'auto';
  direction?: 'row' | 'row-reverse';
  wrap?: 'nowrap' | 'wrap' | 'wrap-reverse';
  nowrap?: boolean;
  gap?: string;
  rowGap?: string;
  colGap?: string;
  justifyItems?:
    | 'normal'
    | 'stretch'
    | 'baseline'
    | 'first baseline'
    | 'last baseline'
    | 'start'
    | 'end'
    | 'legacy'
    | 'legacy'
    | 'left'
    | 'right'
    | 'center';
  justifyContent?:
    | 'flex-start'
    | 'flex-end'
    | 'center'
    | 'space-between'
    | 'space-around'
    | 'space-evenly'
    | 'start'
    | 'end'
    | 'left'
    | 'right';
  alignItems?:
    | 'stretch'
    | 'flex-start'
    | 'flex-end'
    | 'center'
    | 'baseline'
    | 'first baseline'
    | 'last baseline'
    | 'start'
    | 'end'
    | 'self-start'
    | 'self-end';
  alignContent?:
    | 'flex-start'
    | 'flex-end'
    | 'center'
    | 'space-between'
    | 'space-around'
    | 'space-evenly'
    | 'stretch'
    | 'start'
    | 'end'
    | 'baseline'
    | 'first baseline'
    | 'last baseline';
  alignSelf?: 'auto' | 'flex-start' | 'flex-end' | 'center' | 'baseline' | 'stretch';
}

/**
 * Provides a way to style an application page in a grid format. Row simply returns a div element with a flex display with a flex
 * direction of type row.
 * @param props0 Component properties.
 * @returns Row component.
 */
export const Row = React.forwardRef<HTMLDivElement, IRowProps>(
  ({ children, direction = 'row', ...rest }, ref) => {
    return (
      <styled.Row direction={direction} ref={ref} {...rest}>
        {children}
      </styled.Row>
    );
  },
);
