import { BasicSearch } from 'components/basic-search';
import { UserProfile } from 'components/user-profile';
import React from 'react';
import { Link, Row, Show, useWindowSize } from 'tno-core';

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
  const { width } = useWindowSize();
  return (
    <styled.Header className="header">
      <Row>
        {showLogo && (
          <Link to="/landing/home" title="MMI Home page" className="logo-container">
            <img
              className="mm-logo"
              src={process.env.PUBLIC_URL + '/assets/MMinsights_logo_black.svg'}
              alt="MMinsights logo"
            />
            <img
              className="mm-logo-no-text"
              src={process.env.PUBLIC_URL + '/assets/mm_logo.svg'}
              alt="MMinsights logo"
              width="80"
            />
          </Link>
        )}
        {children}
      </Row>
      <Show visible={!window.location.pathname.includes('/search') && !!width && width > 900}>
        <BasicSearch inHeader />
      </Show>
      {showProfile && <UserProfile />}
    </styled.Header>
  );
};
