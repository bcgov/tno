import React from 'react';
import { FaFilterCircleXmark } from 'react-icons/fa6';

import {
  FieldSize,
  generateQuery,
  IContentModel,
  IFilterSettingsModel,
  Row,
  Select,
} from '@/shared';
import { dayjs } from '@/shared/utils/date';
import { MsearchMultisearchBody } from '@/shared/utils/elastic/esTypes';
import { ContentList } from '@/subscriber/components/content-list';
import { ContentListActionBar } from '@/subscriber/components/tool-bar';
import { castToSearchResult, createFilterSettings } from '@/subscriber/features/utils';
import { IContentSearchResult } from '@/subscriber/features/utils/interfaces';
import { useContent } from '@/subscriber/store/hooks';
import { useContributors } from '@/subscriber/store/hooks/subscriber/useContributors';

import { IDateOptions, IPressMember } from './interfaces';
import * as styled from './styled';
import { generateDates } from './utils';

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
  const [pressMemberNames, setPressMembersNames] = React.useState<string[]>([]);
  const [pressSettings] = React.useState<IFilterSettingsModel>(
    createFilterSettings(`${dayjs().startOf('day')}`, `${dayjs().subtract(2, 'weeks')}`),
  );

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res = await findContentWithElasticsearch(filter, false);
        setContent(
          res.hits.hits.map((r) => {
            const content = r._source as IContentModel;
            return castToSearchResult(content);
          }),
        );
      } catch {}
    },
    // do not want to trigger on loading change, will cause infinite loop
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );

  React.useEffect(() => {
    const dates = generateDates();
    setDateOptions(dates);
  }, []);

  React.useEffect(() => {
    api.findAllContributors().then((contributors) => {
      setPressMembers(contributors.filter((contributor) => contributor.isPress));
      const names = contributors
        .filter((c) => c.isPress)
        .map((contributor) => {
          return contributor.name;
        });
      setPressMembersNames(names);
    });
    // run on init
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  React.useEffect(() => {
    var startDate = `${dayjs().startOf('day').subtract(2, 'weeks')}`;
    var endDate = `${dayjs()}`;
    // check if the date filters are persisted to run the initial query
    if (!!pressGalleryFilter.dateFilter) {
      startDate = `${dayjs(pressGalleryFilter.dateFilter.value).startOf('day')}`;
      endDate = `${dayjs(pressGalleryFilter.dateFilter.value).endOf('day')}`;
    }
    // only fetch once the Press Members are ready
    if (!!pressMemberNames.length && !initialLoad) {
      fetchResults(
        generateQuery({
          ...pressSettings,
          defaultOperator: 'or',
          pressMembers: pressMemberNames,
          startDate,
          endDate,
        }),
      );
      setInitialLoad(true);
    }
  }, [
    pressMemberNames,
    pressSettings,
    initialLoad,
    fetchResults,
    pressGalleryFilter,
    pressMembers,
  ]);

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  return (
    <styled.PressGallery>
      <ContentListActionBar
        content={selected}
        onClear={() => setSelected([])}
        onSelectAll={(e) => {
          const values = content.map((c) => c.id);
          const oldSelected = selected.filter((s) => !values.includes(s.id));
          e.target.checked ? setSelected([...selected, ...content]) : setSelected(oldSelected);
        }}
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
                defaultOperator: 'or',
                pressMembers: [
                  pressMemberNames.find(
                    (pm) =>
                      pm ===
                      (e.value?.trim().startsWith('"') && e.value.trim().endsWith('"')
                        ? e.value.trim().slice(1, -1)
                        : e.value.trim()),
                  ) ?? '',
                ],
                startDate: pressGalleryFilter.dateFilter
                  ? `${dayjs(pressGalleryFilter.dateFilter.value).startOf('day')}`
                  : `${dayjs().subtract(2, 'weeks')}`,
                endDate: pressGalleryFilter.dateFilter
                  ? `${dayjs(pressGalleryFilter.dateFilter.value).endOf('day')}`
                  : `${dayjs()}`,
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
              label: `${d.label}`,
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
                  defaultOperator: 'or',
                  pressMembers: pressGalleryFilter.pressFilter?.value
                    ? [
                        pressMemberNames.find(
                          (pm) =>
                            pm ===
                            (pressGalleryFilter.pressFilter?.value as string).replace(
                              /^"+|"+$/g,
                              '',
                            ),
                        ) ?? '',
                      ]
                    : (pressMemberNames ?? []),
                  startDate: `${dayjs(e.value).startOf('day')}`,
                  endDate: `${dayjs(e.value).endOf('day')}`,
                }),
              );
            }
          }}
          name="date-select"
          width={FieldSize.Small}
        />
        <FaFilterCircleXmark
          className="reset"
          onClick={() => {
            storeGalleryDateFilter(null);
            storeGalleryPressFilter(null);
            fetchResults(
              generateQuery({
                ...pressSettings,
                defaultOperator: 'or',
                pressMembers: pressMemberNames,
                startDate: `${dayjs().subtract(2, 'weeks')}`,
                endDate: `${dayjs()}`,
              }),
            );
          }}
        />
      </Row>
      <ContentList
        onContentSelected={handleContentSelected}
        content={content}
        showDate
        showTime
        showSeries
        selected={selected}
        highlighTerms={pressMemberNames}
      />
    </styled.PressGallery>
  );
};
