import React from 'react';
import { ReactNode } from 'react';
import { Row } from 'tno-core';

import * as styled from './styled';

export interface IBoxProps extends Omit<React.HtmlHTMLAttributes<HTMLDivElement>, 'title'> {
  icon?: ReactNode;
  title?: ReactNode;
  canShrink?: boolean;
  expand?: boolean;
  onExpand?: (expand: boolean) => boolean;
  actions?: ReactNode;
}

export const Box = React.forwardRef<HTMLDivElement, IBoxProps>(
  (
    {
      icon,
      title,
      canShrink = false,
      expand = true,
      onExpand = (expand) => expand,
      actions,
      children,
      className,
      ...rest
    },
    ref,
  ) => {
    const [show, setShow] = React.useState(expand);

    React.useEffect(() => {
      setShow(expand);
    }, [expand]);

    return (
      <styled.Box
        className={`box${className ? ` ${className}` : ''}`}
        canShrink={canShrink}
        expand={expand}
        {...rest}
      >
        <div className="box-header">
          {icon}
          <Row
            className="box-header-title"
            flex="1"
            onClick={() => (canShrink ? setShow(onExpand?.(!show)) : null)}
          >
            {title && typeof title === 'string' ? <h2>{title}</h2> : title}
          </Row>
          {actions}
        </div>
        {show && <div className="box-body">{children}</div>}
      </styled.Box>
    );
  },
);
