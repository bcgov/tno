import React from 'react';

import * as styled from './styled';

export interface IColProps extends React.HTMLAttributes<HTMLDivElement> {
  flex?: string;
  display?: string;
  grow?: number;
  shrink?: number;
  basis?: string | 'auto';
  direction?: 'column' | 'column-reverse';
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
 * Provides a way to style an application page in a grid format. Col simply returns a div element with a flex display with a flex
 * direction of type column.
 * @param children the elements you wish to have in your column
 * @param style pass further CSS properties to the column
 * @returns
 */
export const Col: React.FC<IColProps> = ({ children, direction = 'column', ...rest }) => {
  return (
    <styled.Col direction={direction} {...rest}>
      {children}
    </styled.Col>
  );
};
