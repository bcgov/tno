import { GroupedTable } from 'components/grouped-table';
import React from 'react';
import { FaAngleLeft, FaAngleRight, FaCalendarAlt } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { ContentTypeName, Page, Row, useWindowSize } from 'tno-core';

import { columns } from './constants';
import { HomeFilters } from './home-filters';
import * as styled from './styled';
import { makeFilter } from './utils';

/**
 * Home component that will be rendered when the user is logged in.
 */
export const Home: React.FC = () => {
  const [{ content, filter }, { findContent }] = useContent();
  const navigate = useNavigate();
  const [, setLoading] = React.useState(false);
  const { width } = useWindowSize();
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
      <Row justifyContent="center" className="date-navigator">
        <FaAngleLeft />
        2023-03-04
        <FaAngleRight />
        <FaCalendarAlt className="calendar" />
      </Row>
      <Row className="table-container">
        <GroupedTable
          onRowClick={(e, row) => {
            navigate(`/view/${row.original.id}`);
          }}
          data={content?.items || []}
          groupBy={!!width && width > 500 ? 'source.name' : 'source.code'}
          columns={columns}
        />
      </Row>
    </styled.Home>
  );
};
