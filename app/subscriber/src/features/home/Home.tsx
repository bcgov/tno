import { DateFilter } from 'components/date-filter';
import { GroupedTable } from 'components/grouped-table';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { Page, Row } from 'tno-core';

import { columns } from './constants';
import { HomeFilters } from './home-filters';
import * as styled from './styled';
import { makeFilter } from './utils';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const [{ content, filter, filterAdvanced }, { findContent }] = useContent();
  const navigate = useNavigate();
  const [, setLoading] = React.useState(false);
  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setLoading(true);
        const data = await findContent(
          makeFilter({
            ...filter,
          }),
        );
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
        <HomeFilters fetch={fetch} />
      </Row>
      <DateFilter fetch={fetch} />
      <Row className="table-container">
        <GroupedTable
          onRowClick={(e, row) => {
            navigate(`/view/${row.original.id}`);
          }}
          data={content?.items || []}
          // TODO: return full source object from API so we can use name or code
          // groupBy={!!width && width > 500 ? 'source.name' : 'source.code'}
          groupBy="otherSource"
          columns={columns}
        />
      </Row>
    </styled.Home>
  );
};
