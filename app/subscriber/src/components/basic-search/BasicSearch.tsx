import { filterFormat } from 'features/search-page/utils';
import { handleEnterPressed, isNumber } from 'features/utils';
import { FaPlay, FaSearch } from 'react-icons/fa';
import { useNavigate } from 'react-router';
import { useParams } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { Button, IFilterSettingsModel, Row, Text, toQueryString } from 'tno-core';

import * as styled from './styled';

export interface IBasicSearchProps {
  onSearch?: (filter: IFilterSettingsModel) => void;
}

/** Basic search functionality (just search term), and an option to get to the advanced filter */
export const BasicSearch = ({ onSearch }: IBasicSearchProps) => {
  const { id } = useParams();
  const [
    {
      search: { filter },
    },
    { storeSearchFilter },
  ] = useContent();
  const navigate = useNavigate();

  const filterId = id && isNumber(id) ? parseInt(id) : '';

  const handleSearch = async () => {
    navigate(`/search?${toQueryString(filterFormat(filter))}`);
    onSearch?.(filter);
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
          value={filter.search ?? ''}
          onChange={(e) => {
            storeSearchFilter({ ...filter, search: e.target.value });
          }}
        />
      </Row>
      <Text
        className="search-mobile"
        name="search-mobile"
        value={filter.search ?? ''}
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
      <p onClick={() => navigate(`/search/advanced/${filterId}`)}>GO ADVANCED</p>
    </styled.BasicSearch>
  );
};
