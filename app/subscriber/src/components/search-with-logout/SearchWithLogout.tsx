import { UserProfile } from 'components/user-profile';
import React from 'react';
import { Col } from 'tno-core';

import * as styled from './styled';

/** The component containing the search bar and the logout button for the subscriber application. Responsive and adjusts to the viewing device*/
export const SearchWithLogout: React.FC = () => {
  return (
    <styled.SearchWithLogout className="search-with-logout">
      <div className="logo-container">
        <img
          className="mm-logo"
          src={process.env.PUBLIC_URL + '/assets/MMinsights_logo_black.svg'}
          alt="MMinsights logo"
        />
      </div>
      <UserProfile />
    </styled.SearchWithLogout>
  );
};
