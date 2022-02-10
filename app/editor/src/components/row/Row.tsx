import React from 'react';

import * as styled from './RowStyled';

/**
 * Provides a way to style an application page in a grid format. Row simply returns a div element with a flex display with a flex
 * direction of type row.
 * @param children the elements you wish to have in your row
 * @param style pass further CSS properties
 * @returns
 */
export const Row: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({ children, style }) => {
  return <styled.Row style={style}>{children}</styled.Row>;
};
