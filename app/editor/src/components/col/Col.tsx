import React from 'react';

import * as styled from './ColStyled';

/**
 * Provides a way to style an application page in a grid format. Col simply returns a div element with a flex display with a flex
 * direction of type column.
 * @param children the elements you wish to have in your column
 * @param style pass further CSS propertie to the column
 * @returns
 */
export const Col: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({ children, style }) => {
  return <styled.Col style={style}>{children}</styled.Col>;
};
