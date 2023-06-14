import { SearchWithLogout } from 'components/search-with-logout';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { DetermineToneIcon, makeFilter } from 'features/home/utils';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { Col, IContentModel, Page, Row } from 'tno-core';

import * as styled from './styled';
import { trimWords } from './utils';

// Simple component to display users search results
export const SearchPage: React.FC = () => {
  const [{ filter, filterAdvanced }, { findContent }] = useContent();
  const [searchItems, setSearchItems] = React.useState<IContentModel[]>([]);
  const navigate = useNavigate();
  const urlParams = new URLSearchParams(window.location.search);
  const queryText = urlParams.get('queryText');
  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        const data = await findContent(
          makeFilter({
            ...filter,
            startDate: '',
            contentTypes: [],
            endDate: '',
          }),
        );
        setSearchItems(data.items);
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      }
    },
    [findContent],
  );

  /** retrigger content fetch when change is applied */
  React.useEffect(() => {
    fetch({
      ...filter,
      ...filterAdvanced,
      keyword: queryText ?? '',
    });
  }, [filter, filterAdvanced, fetch, queryText]);

  return (
    <styled.SearchPage>
      <SearchWithLogout />
      <Col className="search-items">
        {searchItems.map((item) => {
          return (
            <Row key={item.id} className="rows">
              <Col className="cols">
                <Row className="tone-date">
                  <DetermineToneIcon tone={item.tonePools?.length ? item.tonePools[0].value : 0} />
                  <p className="date text-content">{new Date(item.publishedOn).toDateString()}</p>
                </Row>
                <p className="headline text-content" onClick={() => navigate(`/view/${item.id}`)}>
                  {item.headline}
                </p>
                {/* TODO: Extract text around keyword searched and preview that text rather than the first 50 words */}
                <p className="summary text-content">
                  {item.body ? trimWords(item.body, 50) : trimWords(item.summary, 50)}
                </p>
              </Col>
            </Row>
          );
        })}
      </Col>
    </styled.SearchPage>
  );
};
