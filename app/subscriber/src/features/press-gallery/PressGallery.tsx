import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { IContentListAdvancedFilter } from 'features/content/list-view/interfaces';
import { determineColumns } from 'features/home/constants';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { useContributors } from 'store/hooks/subscriber/useContributors';
import { FlexboxTable, IContentModel, IContributorModel, ITableInternalRow, Row } from 'tno-core';

import * as styled from './styled';

export const PressGallery: React.FC = () => {
  const navigate = useNavigate();
  const [{ filterAdvanced }, { findContentWithElasticsearch }] = useContent();
  const [, api] = useContributors();
  const [pressContributors, setPressContributors] = React.useState<IContributorModel[]>([]);
  const [results, setResults] = React.useState<any>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  const query = (queryText: string, filterAdvanced: IContentListAdvancedFilter) => {
    var query = {
      query: {
        bool: {
          should: [
            {
              range: {
                publishedOn: {
                  gte: `${moment(filterAdvanced.startDate).format('YYYY-MM-DD')}`,
                  lte: `${moment(filterAdvanced.startDate).format('YYYY-MM-DD')}`,
                  time_zone: 'US/Pacific',
                  format: 'yyyy-MM-DD',
                },
              },
            },
            {
              multi_match: {
                query: `${queryText}`,
                fields: ['headline^5', 'byline', 'summary', 'body'],
                default_operator: 'or',
              },
            },
          ],
        },
      },
    };
    return query;
  };

  const fetchResults = React.useCallback(
    async (filter: unknown) => {
      try {
        const res: any = await findContentWithElasticsearch(filter);
        setResults(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  React.useEffect(() => {
    api
      .findAllContributors()
      .then((data) => setPressContributors(data.filter((contributor) => contributor.isPress)))
      .catch();
    // Only want to run this once, so we can ignore the dependency array
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    // the following will match words separated by space or comma, and treat words surrounded by quotes as one value
    const regex = /"[^"]+"|\w+/g;
    const names = pressContributors.map((contributor) => {
      if (!!contributor.aliases) {
        return contributor.aliases.match(regex);
      } else {
        return contributor.name;
      }
    });
    fetchResults(query(names.toString(), filterAdvanced));
    // Only want to run when the date is updated
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filterAdvanced]);

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  return (
    <styled.PressGallery>
      <FolderSubMenu selectedContent={selected} />
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
          data={results}
          pageButtons={5}
          onSelectedChanged={handleSelectedRowsChanged}
          showPaging={false}
        />
      </Row>
    </styled.PressGallery>
  );
};
