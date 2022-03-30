import React from 'react';

import * as styled from './styled';

export interface IViewProps extends React.HTMLAttributes<HTMLDivElement> {}

export const View: React.FC<IViewProps> = ({ className, children, ...rest }) => {
  return (
    <styled.View className={`view${!!className ? ` ${className}` : ''}`} {...rest}>
      {children}
    </styled.View>
  );
};
