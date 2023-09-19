import { DateFilter } from 'components/date-filter';
import { determineColumns } from 'features/home/constants';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { ActionName, FlexboxTable, IContentModel, Row } from 'tno-core';

import * as styled from './styled';

/** Component that displays commentary defaulting to today's date and adjustable via a date filter. */
export const TodaysCommentary: React.FC = () => {
  const [{ filterAdvanced }, { findContent }] = useContent();
  const navigate = useNavigate();
  const [commentary, setCommentary] = React.useState<IContentModel[]>([]);

  React.useEffect(() => {
    findContent({
      actions: [ActionName.Commentary],
      contentTypes: [],
      publishedStartOn: moment(filterAdvanced.startDate).toISOString(),
      publishedEndOn: moment(filterAdvanced.endDate).toISOString(),
      quantity: 100,
    }).then((data) => setCommentary(data.items));
  }, [findContent, filterAdvanced]);

  return (
    <styled.TodaysCommentary>
      <DateFilter />
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns('all')}
          isMulti
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          data={commentary || []}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.TodaysCommentary>
  );
};
