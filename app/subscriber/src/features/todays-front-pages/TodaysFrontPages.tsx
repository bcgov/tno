import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { determineColumns } from 'features/home/constants';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent, useFilters, useLookup } from 'store/hooks';
import { FlexboxTable, IContentModel, IFilterModel, ITableInternalRow, Row } from 'tno-core';

import { defaultFilter } from './constants';
import * as styled from './styled';

/** Component that displays front pages defaulting to today's date and adjustable via a date filter. */
export const TodaysFrontPages: React.FC = () => {
  const [, { findContentWithElasticsearch }] = useContent();
  const navigate = useNavigate();
  const [frontpages, setFrontPages] = React.useState<IContentModel[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [{ settings }] = useLookup();
  const [frontpageFilterId, setFrontpageFilterId] = React.useState('');
  const [, { getFilter }] = useFilters();

  const [filter, setFilter] = React.useState<IFilterModel>(defaultFilter);
  const [results, setResults] = React.useState<any>([]);

  const fetchResults = React.useCallback(
    async (filter: unknown) => {
      try {
        const res: unknown = await findContentWithElasticsearch(filter, false);
        setResults(res);
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  React.useEffect(() => {
    const id = settings.find((s) => s.name === 'FrontpageFilter')?.value;
    if (id) setFrontpageFilterId(id);
  }, [settings]);

  React.useEffect(() => {
    if (!!frontpageFilterId && filter?.id !== parseInt(frontpageFilterId)) {
      const id = parseInt(frontpageFilterId);
      setFilter({ ...defaultFilter, id }); // Do this to stop double fetch.
      getFilter(id).then((data) => {
        fetchResults(data.query);
      });
    }
  }, [frontpageFilterId, filter?.id, getFilter, filter.query, fetchResults]);

  React.useEffect(() => {
    const mappedResults = results.hits?.hits?.map((h: { _source: IContentModel }) => {
      const content = h._source;
      return {
        id: content.id,
        headline: content.headline,
        section: content.section,
        tonePools: content.tonePools,
        otherSource: content.otherSource,
        source: content.source,
        page: content.page,
      };
    });
    setFrontPages(mappedResults);
  }, [results]);

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  return (
    <styled.TodaysFrontPages>
      <FolderSubMenu selectedContent={selected} />
      <DateFilter />
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns('all')}
          isMulti
          onSelectedChanged={handleSelectedRowsChanged}
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          data={frontpages || []}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.TodaysFrontPages>
  );
};
