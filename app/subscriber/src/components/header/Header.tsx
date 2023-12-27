import { UserProfile } from 'components/user-profile';
import React from 'react';
import { Row } from 'tno-core';

import * as styled from './styled';

export interface IHeaderProps {
  /** Children to include in the header. */
  children?: React.ReactNode;
  /** Whether to show the user profile component. */
  showProfile?: boolean;
  /** Whether to show the MMinsights logo. */
  showLogo?: boolean;
}

/**
 * Displays a header with children and user profile.
 * @param param0 Component properties.
 * @returns Component
 */
export const Header: React.FC<IHeaderProps> = ({
  children,
  showProfile = true,
  showLogo = true,
}) => {
  return (
    <styled.Header className="header">
      <Row flex="1">
        {showLogo && (
          <div className="logo-container">
            <img
              className="mm-logo"
              src={process.env.PUBLIC_URL + '/assets/MMinsights_logo_black.svg'}
              alt="MMinsights logo"
            />
          </div>
        )}
        {children}
      </Row>

      {showProfile && <UserProfile />}
    </styled.Header>
  );
};
