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

/**
 * Provides a way to style an application page in a grid format. Row simply returns a div element with a flex display with a flex
 * direction of type row.
 * @param children the elements you wish to have in your row
 * @param style pass further CSS properties
 * @returns
 */
export const Row: React.FC<IRowProps> = ({ children, style }) => {
  return <styled.Row style={style}>{children}</styled.Row>;
};
