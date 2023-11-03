import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { determineColumns } from 'features/home/constants';
import { castToSearchResult } from 'features/utils';
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

import { IDateOptions, IPressMember } from './interfaces';
import * as styled from './styled';
import { createFilterSettings, generateDates } from './utils';

export const PressGallery: React.FC = () => {
  const navigate = useNavigate();
  const [{ filterAdvanced }, { findContentWithElasticsearch }] = useContent();
  const [, api] = useContributors();
  const [results, setResults] = React.useState<IContentModel[]>();
  const [pressMembers, setPressMembers] = React.useState<IPressMember[]>([]);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [dateOptions, setDateOptions] = React.useState<IDateOptions[]>([]);
  const [aliases, setAliases] = React.useState<string[]>([]);

  const [dateValue, setDateValue] = React.useState<IOptionItem | null>();
  const [pressValue, setPressValue] = React.useState<IOptionItem | null>();

  const [pressSettings] = React.useState<IFilterSettingsModel>(
    createFilterSettings(`${moment().startOf('day')}`, `${moment().subtract('2', 'weeks')}`),
  );

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res = await findContentWithElasticsearch(filter, false);
        console.log(res);
        setResults(
          res.hits.hits.map((r) => {
            const content = r._source as IContentModel;
            return castToSearchResult(content);
          }),
        );
        // setResults(res.hits.hits.map((h: { _source: IContentModel }) => h._source));
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  /** separate requests to find total hits for each press member */
  const fetchResultHits = React.useCallback(
    async (filter: MsearchMultisearchBody, name?: string, date?: string) => {
      try {
        const res = await findContentWithElasticsearch(filter, false);
        if (!!name)
          setPressMembers((pressMembers) =>
            pressMembers.map((c) =>
              c.name === name ? { ...c, hits: (res.hits.total as any).value } : c,
            ),
          );
        if (!!date)
          setDateOptions((dates) =>
            dates.map((d) =>
              d.value === date ? { ...d, hits: (res.hits.total as any).value } : d,
            ),
          );
      } catch {}
    },
    [findContentWithElasticsearch],
  );

  React.useEffect(() => {
    const dates = generateDates();
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
    // only fetch once the aliases are ready
    !!aliases.length &&
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
    !!aliases.length &&
      dateOptions.forEach((date) => {
        fetchResultHits(
          generateQuery({
            ...pressSettings,
            defaultSearchOperator: 'or',
            search: aliases.toString().split(',').join(' '),
            startDate: `${moment(date.value).startOf('day')}`,
            endDate: `${moment(date.value).endOf('day')}`,
          }),
          '',
          String(date.value),
        );
      });
    // only want to run when date options/ aliases are loaded
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dateOptions.length, aliases, pressSettings]);

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
              value: c.aliases ?? c.name,
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
          data={results !== undefined ? results : []}
          pageButtons={5}
          onSelectedChanged={handleSelectedRowsChanged}
          showPaging={false}
        />
      </Row>
    </styled.PressGallery>
  );
};
