import { UserProfile } from 'components/user-profile';
import React from 'react';

import * as styled from './styled';

/** The component containing the search bar and the logout button for the subscriber application. Responsive and adjusts to the viewing device*/
export const SearchWithLogout: React.FC = () => {
  return (
    <styled.SearchWithLogout className="search-with-logout">
      <UserProfile />
    </styled.SearchWithLogout>
  );
};
