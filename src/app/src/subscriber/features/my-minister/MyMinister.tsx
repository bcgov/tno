import React from 'react';

import { Checkbox, generateQuery, IContentModel, Row } from '@/shared';
import { dayjs } from '@/shared/utils/date';
import { MsearchMultisearchBody } from '@/shared/utils/elastic/esTypes';
import { ContentList } from '@/subscriber/components/content-list';
import { DateFilter } from '@/subscriber/components/date-filter';
import { ContentListActionBar } from '@/subscriber/components/tool-bar';
import { filterFormat } from '@/subscriber/features/search-page/utils';
import { castToSearchResult } from '@/subscriber/features/utils';
import { IContentSearchResult } from '@/subscriber/features/utils/interfaces';
import { useApp, useContent } from '@/subscriber/store/hooks';
import { IMinisterModel } from '@/subscriber/store/hooks/subscriber/interfaces/IMinisterModel';
import { useMinisters } from '@/subscriber/store/hooks/subscriber/useMinisters';
import { useProfileStore } from '@/subscriber/store/slices';

import * as styled from './styled';

export const MyMinister: React.FC = () => {
  const [
    {
      myMinister: { filter },
    },
    { findContentWithElasticsearch, storeMyMinisterFilter: storeFilter },
  ] = useContent();
  const [{ userInfo }] = useApp();
  const [, api] = useMinisters();
  const [{ impersonate }] = useProfileStore();

  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [ministers, setMinisters] = React.useState<IMinisterModel[]>([]);
  const [userMinisters, setUserMinisters] = React.useState<IMinisterModel[]>([]);

  const getAliases = (minister: IMinisterModel) =>
    (minister.aliases ?? '')
      .split(',')
      .map((a) => a.trim())
      .filter((a) => !!a);

  const getSearchTerms = (minister: IMinisterModel) => [...getAliases(minister), minister.name];

  const getMentionLabel = (minister: IMinisterModel) => getAliases(minister)[0] ?? minister.name;

  const makeSimpleQueryString = (terms: string[]) => {
    if (terms.length === 1) return `"${terms[0]}"`;
    const allButLastTerm = terms.slice(0, terms.length - 1);
    const lastTerm = terms[terms.length - 1];
    return allButLastTerm.map((t) => `"${t}"`).join(' | ') + ` | "${lastTerm}"`;
  };

  React.useEffect(
    () => {
      if (userInfo && !ministers.length) {
        api.getMinisters().then((data) => {
          setMinisters(data);
        });
      }
    },
    // do not want to trigger on loading change, will cause infinite loop
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [userInfo],
  );

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        const res = await findContentWithElasticsearch(filter, false);
        const content = res.hits.hits.map((r) => {
          const content = r._source as IContentModel;
          const result = castToSearchResult(content);
          return result;
        });
        return content;
      } catch {}
    },
    // do not want to trigger on loading change, will cause infinite loop
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );

  const handleContentSelected = React.useCallback((content: IContentModel[]) => {
    setSelected(content);
  }, []);

  React.useEffect(() => {
    const displayData = async () => {
      const baseProfile = impersonate ?? userInfo;
      const ministerIds: number[] = baseProfile?.preferences?.myMinisters ?? [];
      if (!ministerIds.length || !ministers.length) return;

      const ministerModels = ministers
        .filter((m) => ministerIds.includes(m.id))
        .map((m) => ({ ...m, contentCount: 0 }))
        .sort((a, b) => (`${a.name}` > `${b.name}` ? 1 : -1));
      const startDate = filter.startDate
        ? filter.startDate
        : dayjs(new Date()).startOf('day').toISOString();
      const endDate = filter.endDate
        ? filter.endDate
        : dayjs(new Date()).endOf('day').toISOString();

      // A content item is associated with every minister whose search returned it.
      const contentById = new Map<number, IContentSearchResult>();
      for (const minister of ministerModels) {
        const res = await fetchResults(
          generateQuery(
            filterFormat({
              ...filter,
              search: makeSimpleQueryString(getSearchTerms(minister)),
              startDate,
              endDate,
              inByline: true,
              inHeadline: true,
              inStory: true,
              defaultOperator: 'or',
            }),
          ),
        );
        res?.forEach((item) => {
          const existing = contentById.get(item.id);
          const result = existing ?? { ...item, ministerName: minister.name, ministerIds: [] };
          if (!result.ministerIds?.includes(minister.id)) {
            result.ministerIds = [...(result.ministerIds ?? []), minister.id];
            result.ministerMentions = [
              ...(result.ministerMentions ?? []),
              getMentionLabel(minister),
            ];
            minister.contentCount += 1;
          }
          if (!existing) contentById.set(item.id, result);
        });
      }

      setContent(Array.from(contentById.values()));
      // Keep the SHOW selections when the date changes.
      setUserMinisters((current) =>
        ministerModels.map((m) => ({
          ...m,
          hide: current.find((c) => c.id === m.id)?.hide,
        })),
      );
    };
    displayData().catch(() => {});
    // only fire when filter changes or ministers updated
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    userInfo?.preferences?.myMinisters,
    impersonate?.preferences?.myMinisters,
    ministers,
    fetchResults,
    filter,
  ]);

  // Only show content associated with at least one of the ministers that are checked.
  const filteredContent = React.useMemo(() => {
    const hiddenIds = userMinisters.filter((m) => m.hide).map((m) => m.id);
    if (!hiddenIds.length) return content;
    return content.filter((c) => c.ministerIds?.some((id) => !hiddenIds.includes(id)));
  }, [content, userMinisters]);

  const hideGroup = (minister: IMinisterModel, hide: boolean) => {
    setUserMinisters((current) => current.map((m) => (m.id === minister.id ? { ...m, hide } : m)));
  };

  return (
    <styled.MyMinister>
      <ContentListActionBar
        content={selected}
        onClear={() => setSelected([])}
        onSelectAll={(e) => {
          const values = filteredContent.map((c) => c.id);
          const oldSelected = selected.filter((s) => !values.includes(s.id));
          e.target.checked
            ? setSelected([...oldSelected, ...filteredContent])
            : setSelected(oldSelected);
        }}
      />
      <DateFilter
        date={filter.startDate}
        onChangeDate={(start, end) =>
          storeFilter({ ...filter, startDate: start, endDate: end, dateOffset: undefined })
        }
      />
      <div className="ministerCheckboxes">
        <span className="option">SHOW:</span>
        {userMinisters.map((m) => {
          return (
            <React.Fragment key={m.id}>
              <Checkbox
                key={m.id}
                className="option"
                checked={!m.hide}
                onChange={(e) => {
                  hideGroup(m, !(e.target as HTMLInputElement).checked);
                }}
              />
              <span className="check-label">{`${m.name} (${m.contentCount})`}</span>
            </React.Fragment>
          );
        })}
      </div>
      <Row className="table-container">
        <ContentList
          content={filteredContent}
          onContentSelected={handleContentSelected}
          selected={selected}
        />
      </Row>
    </styled.MyMinister>
  );
};
