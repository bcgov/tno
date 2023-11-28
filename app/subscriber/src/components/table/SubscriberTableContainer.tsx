import React from 'react';

import * as styled from './styled';

export interface ISubscriberTableContainerProps {
  children?: React.ReactNode;
}

/** Container wrapper to apply around subscriber tables for a quick way to apply theme
 * Primary icon colour applied to al svgs, if darker icon is needed apply classname darker-icon to svg
 * @param children Child components (table)
 */
export const SubscriberTableContainer: React.FC<ISubscriberTableContainerProps> = ({
  children,
}) => {
  return <styled.SubscriberTableContainer>{children}</styled.SubscriberTableContainer>;
};
