import React from 'react';

import * as styled from './styled';

export interface ICenterProps extends React.HtmlHTMLAttributes<HTMLDivElement> {}

/** Simple component used for centering text/items within a cell of a table */
export const Center: React.FC<ICenterProps> = ({ children }) => {
  return <styled.Center className="center">{children}</styled.Center>;
};
