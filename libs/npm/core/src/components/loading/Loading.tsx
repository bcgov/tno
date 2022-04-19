import React from 'react';

import { Spinner } from '..';
import * as styled from './LoadingStyled';

export interface ILoadingProps {
  /** Size of spinner */
  size?: string;
  /** children to display in the loading overlay */
  children?: React.ReactNode;
}

/**
 * Loading provides an overlay with a spinner to indicate to the user something is loading.
 * @param props Div element attributes.
 * @returns Loading component.
 */
export const Loading: React.FC<ILoadingProps> = ({ size = '4rem', children }) => {
  return (
    <styled.Loading>
      <Spinner size={size} />
      {children}
    </styled.Loading>
  );
};
