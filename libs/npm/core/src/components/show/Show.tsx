import React from 'react';

export interface IShowProps {
  /** Whether the children will be visible. */
  visible?: boolean;
  /** Children nodes to display. */
  children: React.ReactNode | ((props: IShowProps) => React.ReactNode);
}

/**
 * A Show component simply wraps conditional elements in a component.
 * Reducing conditional logical statements within JSX.
 * @param param0 Component properties.
 * @returns A new instance of a Show component.
 */
export const Show: React.FC<IShowProps> = ({ visible, children }) => {
  return !!visible ? (
    <>
      {typeof children === 'function'
        ? (children as (props: IShowProps) => React.ReactNode)({ visible, children })
        : children}
    </>
  ) : null;
};
