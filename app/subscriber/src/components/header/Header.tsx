import { UserProfile } from 'components/user-profile';
import React from 'react';
import { Row } from 'tno-core';

import * as styled from './styled';

export interface IHeaderProps {
  /** Children to include in the header. */
  children?: React.ReactNode;
  /** Whether to show the user profile component. */
  showProfile?: boolean;
}

/**
 * Displays a header with children and user profile.
 * @param param0 Component properties.
 * @returns Component
 */
export const Header: React.FC<IHeaderProps> = ({ children, showProfile = true }) => {
  return (
    <styled.Header>
      <Row flex="1">{children}</Row>
      {showProfile && <UserProfile />}
    </styled.Header>
  );
};
