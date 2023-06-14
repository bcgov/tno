import React from 'react';
import { BiLogOut } from 'react-icons/bi';
import { FaSearch, FaUserCircle } from 'react-icons/fa';
import { useNavigate, useParams } from 'react-router-dom';
import { Button, Row, Show, Text, useKeycloakWrapper, useWindowSize } from 'tno-core';

import * as styled from './styled';

/** The component containing the search bar and the logout button for the subscriber application. Responsive and adjusts to the viewing device*/
export const SearchWithLogout: React.FC = () => {
  const keycloak = useKeycloakWrapper();
  const [searchItem, setSearchItem] = React.useState<string>('');
  const navigate = useNavigate();
  const { width } = useWindowSize();
  const { id } = useParams();

  // update search item when id changes on search page
  React.useEffect(() => {
    if (window.location.pathname.includes('search')) {
      if (!!id) setSearchItem(id ?? '');
    }
  }, [id]);
  return (
    <styled.SearchWithLogout>
      <Row>
        <Text
          placeholder="Search news"
          className="search"
          width={'30em'}
          autoComplete="off"
          name="search"
          value={searchItem}
          onChange={(e) => {
            setSearchItem(e.target.value);
          }}
        />
        <Button className="search-button" onClick={() => navigate(`/search/${searchItem}`)}>
          <FaSearch />
        </Button>
      </Row>
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
