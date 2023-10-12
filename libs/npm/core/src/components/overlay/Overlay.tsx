import React from 'react';

import * as styled from './styled';

export interface IOverlayProps extends React.HTMLAttributes<HTMLDivElement> {}

export const Overlay: React.FC<IOverlayProps> = (props) => {
  return <styled.Overlay {...props}></styled.Overlay>;
};
