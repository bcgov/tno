import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { determineColumns } from 'features/home/constants';
import moment from 'moment';
import React from 'react';
import { FiRefreshCcw } from 'react-icons/fi';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { useContributors } from 'store/hooks/subscriber/useContributors';
import {
  FieldSize,
  FlexboxTable,
  generateQuery,
  IContentModel,
  IFilterSettingsModel,
  IOptionItem,
  ITableInternalRow,
  Row,
  Select,
} from 'tno-core';

import * as styled from './styled';
import { createFilterSettings } from './utils';

export const PressGallery: React.FC = () => {
  const navigate = useNavigate();
  const [{ filterAdvanced }, { findContentWithElasticsearch }] = useContent();
  const [, api] = useContributors();
  const [results, setResults] = React.useState<any>([]);
  const [pressMembers, setPressMembers] = React.useState<any[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [dateOptions, setDateOptions] = React.useState<any[]>([]);
  const [aliases, setAliases] = React.useState<any[]>([]);

  const [dateValue, setDateValue] = React.useState<IOptionItem | null>();
  const [pressValue, setPressValue] = React.useState<IOptionItem | null>();

  const [pressSettings] = React.useState<IFilterSettingsModel>(
    createFilterSettings(`${moment().startOf('day')}`, `${moment().subtract('2', 'weeks')}`),
  );

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res: any = await findContentWithElasticsearch(filter, false);
        setResults(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  React.useEffect(() => {
    // create for loop with a cap of 7 days
    let dates: any[] = [];
    for (let i = 0; i < 7; i++) {
      dates.push({
        label: `${moment().subtract(i, 'days').format('YYYY-MM-DD')}`,
        value: `${moment().subtract(i, 'days')}`,
      });
    }
    setDateOptions(dates);
  }, []);

  React.useEffect(() => {
    api.findAllContributors().then((contributors) => {
      setPressMembers(contributors.filter((contributor) => contributor.isPress));
      const allAliases = contributors
        .filter((c) => c.isPress)
        .map((contributor) => {
          if (!!contributor.aliases) {
            return contributor.aliases;
          } else {
            return contributor.name;
          }
        });
      setAliases(allAliases);
    });
    // run on init
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    fetchResults(
      generateQuery({
        ...pressSettings,
        defaultSearchOperator: 'or',
        search: aliases.toString().split(',').join(' '),
        startDate: `${moment().startOf('day').subtract(2, 'weeks')}`,
        endDate: `${moment()}`,
      }),
    );
  }, [aliases, pressSettings, fetchResults]);

  React.useEffect(() => {
    pressMembers.forEach((contributor) => {
      fetchResultHits(
        generateQuery({
          ...pressSettings,
          search: contributor.aliases,
          startDate: `${moment(filterAdvanced.startDate).subtract(2, 'weeks')}`,
          endDate: `${moment()}`,
        }),
        contributor.name,
      );
    });
    // only want to run when press members are loaded
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pressMembers.length]);

  React.useEffect(() => {
    dateOptions.forEach((date) => {
      fetchResultHits(
        generateQuery({
          ...pressSettings,
          search: aliases.toString().split(',').join(' '),
          startDate: `${moment(date.value).startOf('day')}`,
          endDate: `${moment(date.value).endOf('day')}`,
        }),
        '',
        date.value,
      );
    });
    // only want to run when date options are loaded
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dateOptions.length]);

  /** separate requests to find total hits for each press member */
  const fetchResultHits = React.useCallback(
    async (filter: MsearchMultisearchBody, name?: string, date?: string) => {
      try {
        const res: any = await findContentWithElasticsearch(filter, false);
        if (!!name)
          setPressMembers((pressMembers) =>
            pressMembers.map((c) => (c.name === name ? { ...c, hits: res.hits.total.value } : c)),
          );
        if (!!date)
          setDateOptions((dates) =>
            dates.map((d) => (d.value === date ? { ...d, hits: res.hits.total.value } : d)),
          );
      } catch {}
    },
    [findContentWithElasticsearch],
  );

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
      <Row className="tool-bar">
        <Select
          width={FieldSize.Medium}
          name="select-press-member"
          placeholder={'Select a press member'}
          isClearable={false}
          clearValue={() => setPressValue(null)}
          value={pressValue}
          onChange={(e: any) => {
            if (!!e.value) {
              setPressValue(e);
              // can only use one of the two filters
              setDateValue(null);
              fetchResults(
                generateQuery({
                  ...pressSettings,
                  defaultSearchOperator: 'or',
                  search: pressMembers.find((c) => c.name === e.value)?.aliases ?? '',
                  startDate: `${moment().subtract(2, 'weeks')}`,
                  endDate: `${moment()}`,
                }),
              );
            }
          }}
          options={pressMembers.map((c) => {
            return {
              label: `${c.name} (${c.hits ?? 0})`,
              value: c.alias ?? c.name,
            };
          })}
        />
        <p className="or">or</p>
        <Select
          value={dateValue}
          isClearable={false}
          options={dateOptions.map((d) => {
            return {
              label: `${d.label} (${d.hits ?? 0})`,
              value: d.value,
            };
          })}
          placeholder={'Select a date'}
          clearValue={() => setDateValue(undefined)}
          onChange={(e: any) => {
            if (!!e.value) {
              setDateValue(e);
              // can only use one of the two filters
              setPressValue(null);
              fetchResults(
                generateQuery({
                  ...pressSettings,
                  defaultSearchOperator: 'or',
                  search: aliases.toString().split(',').join(' '),
                  startDate: `${moment(e.value).startOf('day')}`,
                  endDate: `${moment(e.value).endOf('day')}`,
                }),
              );
            }
          }}
          name="date-select"
          width={FieldSize.Medium}
        />
        <FiRefreshCcw
          className="reset"
          onClick={() => {
            setDateValue(null);
            setPressValue(null);
            fetchResults(
              generateQuery({
                ...pressSettings,
                defaultSearchOperator: 'or',
                search: aliases.toString().split(',').join(' '),
                startDate: `${moment(filterAdvanced.startDate).subtract(2, 'weeks')}`,
                endDate: `${moment()}`,
              }),
            );
          }}
        />
        <FolderSubMenu selectedContent={selected} />
      </Row>
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
