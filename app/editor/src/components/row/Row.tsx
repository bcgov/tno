import React from 'react';
import { CSSProperties } from 'styled-components';

import * as styled from './RowStyled';

export interface IRowProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * place desired nodes in row
   */
  children?: React.ReactNode;
  /**
   * additional styling flexibility
   */
  style?: CSSProperties;
}

export const Row: React.FC<IRowProps> = ({ children, style }) => {
  return <styled.Row style={style}>{children}</styled.Row>;
};
