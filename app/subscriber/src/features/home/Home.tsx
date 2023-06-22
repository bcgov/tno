import { DateFilter } from 'components/date-filter';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { ContentStatus, ContentTypeName, FlexboxTable, IContentModel, Page, Row } from 'tno-core';

import { determinecolumns } from './constants';
import { HomeFilters } from './home-filters';
import * as styled from './styled';
import { makeFilter } from './utils';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const [{ filter, filterAdvanced }, { findContent }] = useContent();
  const [homeItems, setHomeItems] = React.useState<IContentModel[]>([]);
  const navigate = useNavigate();

  const [, setLoading] = React.useState(false);
  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setLoading(true);
        const data = await findContent(
          makeFilter({
            ...filter,
            contentTypes:
              filter.contentTypes.length > 0 ? filter.contentTypes : [ContentTypeName.PrintContent],
            startDate: filter.startDate ? filter.startDate : new Date().toDateString(),
            sort: [{ id: 'sourceSort' }],
            status: ContentStatus.Published,
          }),
        );
        setHomeItems(data.items);
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      } finally {
        setLoading(false);
      }
    },
    [findContent],
  );

  /** retrigger content fetch when change is applied */
  React.useEffect(() => {
    fetch({ ...filter, ...filterAdvanced });
  }, [filter, filterAdvanced, fetch]);

  return (
    <styled.Home>
      <Row>
        <div className="show-media-label">SHOW MEDIA TYPE:</div>
        <HomeFilters />
      </Row>
      <DateFilter />
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determinecolumns(filter.contentTypes[0])}
          isMulti
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          data={homeItems}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.Home>
  );
};
