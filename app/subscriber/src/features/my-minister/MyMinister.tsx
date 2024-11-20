import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentList } from 'components/content-list';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useApp, useContent } from 'store/hooks';
import { IMinisterModel } from 'store/hooks/subscriber/interfaces/IMinisterModel';
import { useMinisters } from 'store/hooks/subscriber/useMinisters';
import { useProfileStore } from 'store/slices';
import { Checkbox, generateQuery, IContentModel, Row } from 'tno-core';

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
  const [filteredContent, setFilteredContent] = React.useState<IContentSearchResult[]>([]);
  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [ministers, setMinisters] = React.useState<IMinisterModel[]>([]);
  const [userMinisters, setUserMinisters] = React.useState<IMinisterModel[]>([]);

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
    const fillMentions = (r: IContentSearchResult, ministerName: string) => {
      const mentions: string[] = [];
      ministers
        .filter((m) => userInfo?.preferences?.myMinisters?.includes(m.id))
        // eslint-disable-next-line array-callback-return
        .map((m: IMinisterModel) => {
          let includeMention = false;
          const aliases = m.aliases.split(',');
          for (let i = 0; i <= aliases.length; i++) {
            if (r.summary.includes(aliases[i]) || r.body.includes(aliases[i])) {
              includeMention = true;
            }
          }
          if (r.summary.includes(m.name) || r.body.includes(m.name)) {
            includeMention = true;
          }
          if (includeMention) {
            mentions.push(!!aliases ? aliases[0] : m.name);
          }
        });
      return mentions;
    };

    const displayData = async () => {
      const contentList: IContentSearchResult[] = [];
      if (
        userInfo?.preferences?.myMinisters?.length > 0 ||
        (impersonate?.preferences?.myMinisters?.length > 0 && ministers.length > 0)
      ) {
        const baseProfile = impersonate ?? userInfo;
        let ministerModels = ministers
          .filter((m) => baseProfile?.preferences?.myMinisters?.includes(m.id))
          .map((m) => {
            return { ...m, contentCount: 0 };
          })
          .sort((a: IMinisterModel, b: IMinisterModel) => (`${a.name}` > `${b.name}` ? 1 : -1));
        const startDate = filter.startDate
          ? filter.startDate
          : moment(new Date()).startOf('day').toISOString();
        const endDate = filter.endDate
          ? filter.endDate
          : moment(new Date()).endOf('day').toISOString();
        if (ministerModels) {
          for (let j = 0; j < ministerModels.length; j++) {
            const res = await fetchResults(
              generateQuery(
                filterFormat({
                  ...filter,
                  search: makeSimpleQueryString([
                    ...ministerModels[j].aliases.split(','),
                    ministerModels[j].name,
                  ]),
                  startDate,
                  endDate,
                  inByline: true,
                  inHeadline: true,
                  inStory: true,
                  defaultOperator: 'or',
                }),
              ),
            );
            if (!!res && res.length > 0) {
              for (let i = 0; i < res.length; i++) {
                res[i].ministerName = ministerModels[j].name;
                res[i].ministerMentions = await fillMentions(res[i], ministerModels[j].name);
                ministerModels[j].contentCount += 1;
                contentList.push(res[i]);
              }
            }
          }
          const ids = contentList.map(({ id }) => id);
          const grouped = contentList.filter(({ id }, index) => !ids.includes(id, index + 1));
          setContent(grouped);
          setUserMinisters(ministerModels);
        }
      }
    };
    displayData().catch(() => {});
    // only fire when filter changes or ministers updated
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [userInfo?.preferences?.myMinisters, ministers, fetchResults, filter]);

  React.useEffect(() => {
    setFilteredContent(content);
  }, [content]);

  const hideGroup = (minister: IMinisterModel, hide: boolean) => {
    const ministersList = [...userMinisters];
    const m = ministersList.find((m) => m.id === minister.id);
    if (m) {
      m.hide = hide;
    }
    setUserMinisters(ministersList);
    let filteredContent = [...content];
    const mentionsToHide = ministersList
      .filter((m) => m.hide)
      .map((m) => {
        const aliases = m.aliases.split(',');
        return !!aliases ? aliases[0] : m.name;
      });
    filteredContent = filteredContent.filter((c) => {
      let count = 0;
      mentionsToHide.forEach((h) => {
        if (c.ministerMentions?.includes(h)) {
          count++;
        }
      });
      return count !== c.ministerMentions?.length;
    });
    setFilteredContent(filteredContent);
  };

  return (
    <styled.MyMinister>
      <ContentListActionBar
        content={selected}
        onClear={() => setSelected([])}
        onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
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
