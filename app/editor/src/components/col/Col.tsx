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

/**
 * Provides a way to style an application page in a grid format. Col simply returns a div element with a flex display with a flex
 * direction of type column.
 * @param children the elements you wish to have in your column
 * @param style pass further CSS propertie to the column
 * @returns
 */
export const Col: React.FC<IColProps> = ({ children, style }) => {
  return <styled.Col style={style}>{children}</styled.Col>;
};
