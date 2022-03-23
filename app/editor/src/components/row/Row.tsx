import React from 'react';

import * as styled from './styled';

/**
 * Provides a way to style an application page in a grid format. Row simply returns a div element with a flex display with a flex
 * direction of type row.
 * @returns
 */
export const Row: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({ children, ...rest }) => {
  return <styled.Row {...rest}>{children}</styled.Row>;
};
