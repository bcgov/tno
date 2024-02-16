import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { ContentListActionBar } from 'components/tool-bar';
import { castToSearchResult, createFilterSettings } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { FiRefreshCcw } from 'react-icons/fi';
import { useContent } from 'store/hooks';
import { useContributors } from 'store/hooks/subscriber/useContributors';
import {
  FieldSize,
  generateQuery,
  IContentModel,
  IFilterSettingsModel,
  Row,
  Select,
} from 'tno-core';

import { IDateOptions, IPressMember } from './interfaces';
import * as styled from './styled';
import { generateDates } from './utils';

export const PressGallery: React.FC = () => {
  const [
    {
      home: { filter },
    },
    { findContentWithElasticsearch },
  ] = useContent();
  const [, api] = useContributors();
  const [{ pressGalleryFilter }, { storeGalleryDateFilter, storeGalleryPressFilter }] =
    useContent();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [pressMembers, setPressMembers] = React.useState<IPressMember[]>([]);
  const [initialLoad, setInitialLoad] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [dateOptions, setDateOptions] = React.useState<IDateOptions[]>([]);
  const [aliases, setAliases] = React.useState<string[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [groupedContent, setGroupedContent] = React.useState<any[]>([]);
  const [pressSettings] = React.useState<IFilterSettingsModel>(
    createFilterSettings(`${moment().startOf('day')}`, `${moment().subtract('2', 'weeks')}`),
  );

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        if (!loading) {
          setLoading(true);
          const res = await findContentWithElasticsearch(filter, false);
          setContent(
            res.hits.hits.map((r) => {
              const content = r._source as IContentModel;
              return castToSearchResult(content);
            }),
          );
        }
      } catch {
      } finally {
        setLoading(false);
      }
    },
    // do not want to trigger on loading change, will cause infinite loop
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );

  /** separate requests to find total hits for each press member */
  const fetchResultHits = React.useCallback(
    async (filter: MsearchMultisearchBody, name?: string) => {
      try {
        const res = await findContentWithElasticsearch(filter, false);
        if (!!name)
          setPressMembers((pressMembers) =>
            pressMembers.map((c) =>
              c.name === name ? { ...c, hits: (res.hits.total as any).value } : c,
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

  // group content by date on the frontend instead of addtioanl fetches for each date
  React.useEffect(() => {
    if (!!content.length) {
      const grouped = content.reduce((acc: any, content) => {
        const date = moment(content.publishedOn).format('YYYY-MM-DD');
        if (!acc[date]) {
          acc[date] = [];
        }
        acc[date].push(content);
        return acc;
      }, {});
      setGroupedContent(
        Object.entries(grouped).map(([key, value]) => ({ date: key, content: value })),
      );
    }
  }, [content]);

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
      setAliases(allAliases.map((alias) => `"${alias}"`));
    });
    // run on init
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    var startDate = `${moment().startOf('day').subtract(2, 'weeks')}`;
    var endDate = `${moment()}`;
    // check if the date filters are persisted to run the initial query
    if (!!pressGalleryFilter.dateFilter) {
      startDate = `${moment(pressGalleryFilter.dateFilter.value).startOf('day')}`;
      endDate = `${moment(pressGalleryFilter.dateFilter.value).endOf('day')}`;
    }
    // only fetch once the aliases are ready
    if (!!aliases.length && !initialLoad) {
      var aliasesFilter = aliases.toString().split(',').join(' ');
      if (!!pressGalleryFilter.pressFilter) {
        aliasesFilter =
          pressMembers.find((c) => c.name === pressGalleryFilter.pressFilter?.value)?.aliases ?? '';
      }
      fetchResults(
        generateQuery({
          ...pressSettings,
          defaultSearchOperator: 'or',
          search: aliasesFilter,
          startDate,
          endDate,
        }),
      );
      setInitialLoad(true);
    }
  }, [aliases, pressSettings, fetchResults, pressGalleryFilter, pressMembers]);

  React.useEffect(() => {
    pressMembers.forEach((contributor) => {
      fetchResultHits(
        generateQuery({
          ...pressSettings,
          search: contributor.aliases,
          startDate: `${moment(filter.startDate).subtract(2, 'weeks')}`,
          endDate: `${moment()}`,
        }),
        contributor.name,
      );
    });
    // only want to run when press members are loaded
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pressMembers.length]);

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  return (
    <styled.PressGallery>
      <ContentListActionBar
        content={selected}
        onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
      />
      <Row className="tool-bar">
        <Select
          width={FieldSize.Medium}
          name="select-press-member"
          placeholder={'Select a press member'}
          isClearable={false}
          clearValue={() => storeGalleryPressFilter(null)}
          value={pressGalleryFilter.pressFilter}
          onChange={(e: any) => {
            storeGalleryPressFilter(e);
            fetchResults(
              generateQuery({
                ...pressSettings,
                search: pressMembers.find((c) => c.name === e.value)?.aliases ?? '',
                startDate: pressGalleryFilter.dateFilter
                  ? `${moment(pressGalleryFilter.dateFilter.value).startOf('day')}`
                  : `${moment(filter.startDate).subtract(2, 'weeks')}`,
                endDate: pressGalleryFilter.dateFilter
                  ? `${moment(pressGalleryFilter.dateFilter.value).endOf('day')}`
                  : `${moment()}`,
              }),
            );
          }}
          options={pressMembers.map((c) => {
            return {
              label: `${c.name} (${c.hits ?? 0})`,
              value: c.aliases ?? c.name,
            };
          })}
        />
        <Select
          value={pressGalleryFilter.dateFilter}
          isClearable={false}
          options={dateOptions.map((d) => {
            return {
              label: `${d.label} (${
                groupedContent.find((c) => c.date === d.label)?.content.length ?? 0
              })`,
              value: d.value,
            };
          })}
          placeholder={'Select a date'}
          clearValue={() => storeGalleryDateFilter(null)}
          onChange={(e: any) => {
            if (!!e.value) {
              storeGalleryDateFilter(e);
              fetchResults(
                generateQuery({
                  ...pressSettings,
                  defaultSearchOperator: 'or',
                  search: pressGalleryFilter.pressFilter?.value
                    ? pressMembers.find((c) => c.name === pressGalleryFilter.pressFilter?.value)
                        ?.aliases ?? ''
                    : aliases.toString().split(',').join(' '),
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
            storeGalleryDateFilter(null);
            storeGalleryPressFilter(null);
            fetchResults(
              generateQuery({
                ...pressSettings,
                defaultSearchOperator: 'or',
                search: aliases.toString().split(',').join(' '),
                startDate: `${moment(filter.startDate).subtract(2, 'weeks')}`,
                endDate: `${moment()}`,
              }),
            );
          }}
        />
      </Row>
      <ContentList
        onContentSelected={handleContentSelected}
        content={content}
        selected={selected}
      />
    </styled.PressGallery>
  );
};
