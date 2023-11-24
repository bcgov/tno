import { ScreenSizes } from 'components/layout';
import { UserProfile } from 'components/user-profile';
import React from 'react';
import { FaSearch } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { Button, Row, Show, Text, useWindowSize } from 'tno-core';

import * as styled from './styled';

/** The component containing the search bar and the logout button for the subscriber application. Responsive and adjusts to the viewing device*/
export const SearchWithLogout: React.FC = () => {
  const [searchItem, setSearchItem] = React.useState<string>('');
  const navigate = useNavigate();
  const { width } = useWindowSize();
  const urlParams = new URLSearchParams(window.location.search);
  const queryText = urlParams.get('queryText');

  // update search item when id changes on search page
  React.useEffect(() => {
    if (window.location.pathname.includes('search')) {
      if (!!queryText) setSearchItem(queryText ?? '');
    }
  }, [queryText]);

  /** allow user to hit enter while searching */
  const enterPressed = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      navigate(`/search?queryText=${searchItem}`);
    }
  };
  return (
    <styled.SearchWithLogout className="search-with-logout">
      <Row>
        {/* use original search when in mobile - until mobile advanced ui provided */}
        <Show visible={!!width && width < ScreenSizes.LargeMobile}>
          <Text
            placeholder="Search news"
            className="search"
            width={width && width > 500 ? '30em' : '15em'}
            autoComplete="off"
            name="search"
            onKeyDown={enterPressed}
            value={searchItem}
            onChange={(e) => {
              setSearchItem(e.target.value);
            }}
          />
          <Button
            className="search-button"
            onClick={() => navigate(`/search?queryText=${searchItem}`)}
          >
            <FaSearch />
          </Button>
        </Show>
      </Row>
      <UserProfile />
    </styled.SearchWithLogout>
  );
};
