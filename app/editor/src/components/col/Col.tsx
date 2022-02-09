import React from 'react';
import { CSSProperties } from 'styled-components';

import * as styled from './ColStyled';

export interface IColProps extends React.ReactHTMLElement<HTMLDivElement> {
  /**
   * place desired nodes in row
   */
  children?: React.ReactNode;
  /**
   * additional styling flexibility
   */
  style?: CSSProperties;
}

export const Col: React.FC<IColProps> = ({ children, style }) => {
  return <styled.Col style={style}>{children}</styled.Col>;
};
