import { BasicSearch } from 'components/basic-search';
import { UserProfile } from 'components/user-profile';
import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Row, Show, useWindowSize } from 'tno-core';

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
  const navigate = useNavigate();
  const location = useLocation();
  const [prevPath, setPrevPath] = React.useState<string | null>(null);

  React.useEffect(() => {
    if (location.pathname !== '/') {
      setPrevPath(location.pathname);
    }
  }, [location.pathname]);

  const handleLogoClick = () => {
    if (prevPath) {
      navigate(prevPath);
      window.location.reload();
    } else {
      navigate('/landing/home');
      window.location.reload();
    }
  };

  return (
    <styled.Header className="header">
      <Row>
        {showLogo && (
          <div
            onClick={handleLogoClick}
            title="MMI Home page"
            className="logo-container"
            style={{ cursor: 'pointer' }}
          >
            <img
              className="mm-logo"
              src={'/assets/MMinsights_logo_black.svg'}
              alt="MMinsights logo"
            />
            <img
              className="mm-logo-no-text"
              src={'/assets/mm_logo.svg'}
              alt="MMinsights logo"
              width="80"
            />
          </div>
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
