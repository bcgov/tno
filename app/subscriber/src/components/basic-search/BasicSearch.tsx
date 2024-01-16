import { defaultAdvancedSearch } from 'features/search-page/components/advanced-search/constants';
import { handleEnterPressed, isNumber } from 'features/utils';
import { FaPlay, FaSearch } from 'react-icons/fa';
import { useNavigate } from 'react-router';
import { useParams } from 'react-router-dom';
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
    <styled.BasicSearch $inHeader={inHeader}>
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
