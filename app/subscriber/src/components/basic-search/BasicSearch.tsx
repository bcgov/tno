import { filterFormat } from 'features/search-page/utils';
import { handleEnterPressed } from 'features/utils';
import { FaPlay, FaSearch } from 'react-icons/fa';
import { useNavigate } from 'react-router';
import { useContent } from 'store/hooks';
import { Button, Row, Text, toQueryString } from 'tno-core';

import * as styled from './styled';

/** Basic search functionality (just search term), and an option to get to the advanced filter */
export const BasicSearch: React.FC = () => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter },
  ] = useContent();
  const navigate = useNavigate();

  const handleSearch = async () => {
    navigate(`/search?${toQueryString(filterFormat(filter))}`);
  };

  return (
    <styled.BasicSearch>
      <label>SEARCH FOR: </label>
      <Row className="icon-search">
        <FaSearch onClick={() => handleSearch()} className="search-icon" />
        <Text
          className="search-input"
          onKeyDown={(e) => handleEnterPressed(e, handleSearch)}
          name="search"
          onChange={(e) => {
            storeSearchFilter({ ...filter, search: e.target.value });
          }}
        />
      </Row>
      <Text
        className="search-mobile"
        name="search-mobile"
        onChange={(e) => {
          storeSearchFilter({ ...filter, search: e.target.value });
        }}
      />

      <Button
        onClick={() => {
          handleSearch();
        }}
        className="search-button"
      >
        Search
        <FaPlay />
      </Button>
      <p onClick={() => navigate('/search/advanced')}>GO ADVANCED</p>
    </styled.BasicSearch>
  );
};
