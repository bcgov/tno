import { DateFilter } from 'components/date-filter';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { determineColumns } from 'features/home/constants';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { useContributors } from 'store/hooks/subscriber/useContributors';
import {
  FlexboxTable,
  generateQuery,
  IContentModel,
  IFilterSettingsModel,
  ITableInternalRow,
  Row,
} from 'tno-core';

import * as styled from './styled';

export const PressGallery: React.FC = () => {
  const navigate = useNavigate();
  const [{ filterAdvanced }, { findContentWithElasticsearch }] = useContent();
  const [, api] = useContributors();
  const [results, setResults] = React.useState<any>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);

  const defaultSettings: IFilterSettingsModel = {
    startDate: `${moment(filterAdvanced.startDate).format('YYYY-MM-DD')}`,
    inByline: true,
    inHeadline: true,
    inStory: true,
    searchUnpublished: false,
    defaultOperator: 'or',
  };
  const [pressSettings, setPressSettings] = React.useState<IFilterSettingsModel>(defaultSettings);
  const [pressQuery, setPressQuery] = React.useState<any>();

  /**
   * Update the settings and query values based on the new key=value.
   */
  const updateQuery = React.useCallback(
    (key: string, value: any) => {
      var settings = defaultSettings;
      settings[key] = value;
      if (key === 'dateOffset') {
        settings = { ...settings, startDate: undefined, endDate: undefined };
      } else if (key === 'startDate' || key === 'endDate') {
        settings = { ...settings, dateOffset: undefined };
      }
      const query = generateQuery(settings, pressQuery);
      setPressSettings(settings);
      setPressQuery(query);
    },
    [pressQuery, defaultSettings],
  );

  const fetchResults = React.useCallback(
    async (filter: unknown) => {
      try {
        const res: any = await findContentWithElasticsearch(filter);
        setResults(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  /** Get all the contributors that are marked as press */
  React.useEffect(() => {
    getPressContributorAliases();
    // Only want this to run when the date is updated or on initial load
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filterAdvanced.startDate]);

  // async function to fetch the press enable contributors and update the search query with appropriate aliases
  const getPressContributorAliases = async () => {
    // regex to match words separated by space or comma, and treat words surrounded by quotes as one value
    const regex = /"[^"]+"|\w+/g;
    const contributors = await api.findAllContributors();
    const aliases = contributors
      .filter((contributor) => contributor.isPress)
      .map((contributor) => {
        if (!!contributor.aliases) {
          return contributor.aliases.match(regex);
        } else {
          return contributor.name;
        }
      });
    updateQuery('search', aliases.toString());
    fetchResults(generateQuery(pressSettings, pressQuery));
  };

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
