import React from 'react';

import * as styled from './styled';

export interface ISectionProps extends React.HTMLAttributes<HTMLDivElement> {}

export const Section: React.FC<ISectionProps> = ({ children, ...rest }) => {
  return <styled.Section {...rest}>{children}</styled.Section>;
};
