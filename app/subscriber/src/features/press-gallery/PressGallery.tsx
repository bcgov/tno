import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { ContentListActionBar } from 'components/tool-bar';
import { castToSearchResult, createFilterSettings, formatDate } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { FaFilterCircleXmark } from 'react-icons/fa6';
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

import { IDateOptions, IGroupedDates, IPressMember } from './interfaces';
import * as styled from './styled';
import { generateDates, separateAlias } from './utils';

export const PressGallery: React.FC = () => {
  const [, api] = useContributors();
  const [
    { pressGalleryFilter },
    { storeGalleryDateFilter, findContentWithElasticsearch, storeGalleryPressFilter },
  ] = useContent();

  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [pressMembers, setPressMembers] = React.useState<IPressMember[]>([]);
  // initial load underway no requests at this time
  const [initialLoad, setInitialLoad] = React.useState(false);
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [dateOptions, setDateOptions] = React.useState<IDateOptions[]>([]);
  const [aliases, setAliases] = React.useState<string[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [contentByDate, setContentByDate] = React.useState<IGroupedDates | undefined>();
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

  React.useEffect(() => {
    const dates = generateDates();
    setDateOptions(dates);
  }, []);

  // group content by date on the frontend instead of addtioanl fetches for each date
  React.useEffect(() => {
    if (!!content.length) {
      const grouped = content.reduce((acc: any, content) => {
        const date = formatDate(content.publishedOn);
        if (!acc[date]) {
          acc[date] = [];
        }
        acc[date].push(content);
        return acc;
      }, {});
      setContentByDate(grouped);
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
      const formattedAliases = separateAlias(allAliases);
      setAliases(formattedAliases);
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
      var aliasesFilter = aliases.toString();
      if (!!pressGalleryFilter.pressFilter) {
        aliasesFilter =
          pressMembers.find((c) => c.aliases === pressGalleryFilter.pressFilter?.value)?.aliases ??
          '';
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
  }, [aliases, pressSettings, initialLoad, fetchResults, pressGalleryFilter, pressMembers]);

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
                defaultSearchOperator: 'or',
                search: pressMembers.find((c) => c.aliases === e.value)?.aliases ?? '',
                startDate: pressGalleryFilter.dateFilter
                  ? `${moment(pressGalleryFilter.dateFilter.value).startOf('day')}`
                  : `${moment().subtract(2, 'weeks')}`,
                endDate: pressGalleryFilter.dateFilter
                  ? `${moment(pressGalleryFilter.dateFilter.value).endOf('day')}`
                  : `${moment()}`,
              }),
            );
          }}
          options={pressMembers.map((c) => {
            return {
              label: `${c.name}`,
              value: c.aliases ?? c.name,
            };
          })}
        />
        <Select
          value={pressGalleryFilter.dateFilter}
          key={pressGalleryFilter.pressFilter?.value}
          isClearable={false}
          options={dateOptions.map((d) => {
            return {
              label: `${d.label} ${
                !pressGalleryFilter.dateFilter
                  ? `(${contentByDate?.[d.label as keyof IGroupedDates]?.length ?? 0})`
                  : ''
              }`,
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
                    ? pressMembers.find((c) => c.aliases === pressGalleryFilter.pressFilter?.value)
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
        <FaFilterCircleXmark
          className="reset"
          onClick={() => {
            storeGalleryDateFilter(null);
            storeGalleryPressFilter(null);
            fetchResults(
              generateQuery({
                ...pressSettings,
                defaultSearchOperator: 'or',
                search: aliases.toString(),
                startDate: `${moment().subtract(2, 'weeks')}`,
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
