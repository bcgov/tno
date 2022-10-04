import React from 'react';

import * as styled from './styled';

interface IEllipsisProps extends React.HTMLAttributes<HTMLDivElement> {}

export const Ellipsis: React.FC<IEllipsisProps> = ({ children, ...rest }) => {
  return (
    <styled.Ellipsis className="ellipsis" {...rest}>
      {children}
    </styled.Ellipsis>
  );
};
