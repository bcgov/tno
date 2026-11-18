import React from 'react';

import * as styled from './styled';

interface ICellEllipsisProps extends React.HTMLAttributes<HTMLDivElement> {}

export const CellEllipsis: React.FC<ICellEllipsisProps> = ({ children, ...rest }) => {
  return (
    <styled.Ellipsis className="ellipsis" {...rest}>
      {children}
    </styled.Ellipsis>
  );
};
