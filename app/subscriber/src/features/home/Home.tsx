import { GroupedTable } from 'components/grouped-table';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { ContentTypeName, Page, Row } from 'tno-core';

import { columns } from './constants';
import { HomeFilters } from './home-filters';
import * as styled from './styled';
import { makeFilter } from './utils';
import { DateFilter } from 'components/date-filter';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const [{ content, filter }, { findContent }] = useContent();
  const navigate = useNavigate();
  const [, setLoading] = React.useState(false);
  const fetch = React.useCallback(
    async (filter: any) => {
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

  // defaults to print content
  React.useEffect(() => {
    fetch({ ...filter, contentTypes: [ContentTypeName.PrintContent] });
  }, [fetch, filter]);

  return (
    <styled.Home>
      <Row>
        <div className="show-media-label">SHOW MEDIA TYPE:</div>
        <HomeFilters fetch={fetch} />
      </Row>
      <DateFilter />
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
