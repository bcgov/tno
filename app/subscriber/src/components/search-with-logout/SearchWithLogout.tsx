import { useWindowSize } from 'hooks';
import React from 'react';
import { BiLogOut } from 'react-icons/bi';
import { FaUserCircle } from 'react-icons/fa';
import { useContent } from 'store/hooks';
import { LogicalOperator, Show, Text, useKeycloakWrapper } from 'tno-core';

import * as styled from './styled';

/** The component containing the search bar and the logout button for the subscriber application. Responsive and adjusts to the viewing device*/
export const SearchWithLogout: React.FC = () => {
  const keycloak = useKeycloakWrapper();
  const [, { storeFilterAdvanced }] = useContent();
  const { width } = useWindowSize();
  return (
    <styled.SearchWithLogout>
      <Text
        placeholder="Search news"
        className="search"
        width={'30em'}
        name="search"
        // TODO: Implement search properly, talk with Bobbi
        onChange={(e) => {
          storeFilterAdvanced({
            searchTerm: e.target.value,
            fieldType: 'headline',
            logicalOperator: LogicalOperator.Contains,
          });
        }}
      />
      <div onClick={() => keycloak.instance.logout()} className="logout">
        <Show visible={!!width && width > 500}>
          <FaUserCircle />
          Logout
        </Show>
        <Show visible={!!width && width < 500}>
          <BiLogOut className="logout-icon" />
        </Show>
      </div>
    </styled.SearchWithLogout>
  );
};
