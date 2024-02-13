import { defaultAdvancedSearch } from 'features/search-page/components/advanced-search/constants';
import { handleEnterPressed, isNumber } from 'features/utils';
import { FaSearch } from 'react-icons/fa';
import { FaPlay } from 'react-icons/fa6';
import { useNavigate } from 'react-router';
import { useLocation, useParams } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { Button, IFilterSettingsModel, Row, Text } from 'tno-core';

import * as styled from './styled';

export interface IBasicSearchProps {
  onSearch?: (filter: IFilterSettingsModel) => void;
  /** whether to display the header variant of the search */
  inHeader?: boolean;
}

/** Basic search functionality (just search term), and an option to get to the advanced filter */
export const BasicSearch = ({ onSearch, inHeader }: IBasicSearchProps) => {
  const { pathname } = useLocation();
  const { id } = useParams();
  const [
    {
      search: { filter },
    },
    { storeSearchFilter },
  ] = useContent();
  const navigate = useNavigate();

  // We only extract the param if this is the search page already.
  const filterId = pathname.startsWith('/search/') && id && isNumber(id) ? parseInt(id) : '';

  const handleSearch = async () => {
    navigate(`/search`);
    // reset any saved filter back to a "basic" filter
    // removing any "advanced" filter settings
    const newFilter: IFilterSettingsModel = {
      ...filter,
      ...defaultAdvancedSearch,
      search: filter.search,
    };
    storeSearchFilter(newFilter);
    onSearch?.(newFilter);
  };

  return (
    <styled.BasicSearch inHeader={inHeader}>
      <label>SEARCH FOR: </label>
      <Row className="search-row">
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
          className="mobile-search-input"
          onKeyDown={(e) => handleEnterPressed(e, handleSearch)}
          name="search"
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
      </Row>
      <p onClick={() => navigate(`/search/advanced/${filterId}`)}>GO ADVANCED</p>
    </styled.BasicSearch>
  );
};
