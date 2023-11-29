import { ScreenSizes } from 'components/layout';
import { UserProfile } from 'components/user-profile';
import React from 'react';
import { FaSearch } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { Button, Row, Show, Text, useWindowSize } from 'tno-core';

import * as styled from './styled';

/** The component containing the search bar and the logout button for the subscriber application. Responsive and adjusts to the viewing device*/
export const SearchWithLogout: React.FC = () => {
  return (
    <styled.SearchWithLogout className="search-with-logout">
      <UserProfile />
    </styled.SearchWithLogout>
  );
};
