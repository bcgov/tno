import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { DateFilter } from 'components/date-filter';
import { ContentListActionBar } from 'components/tool-bar';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp, useContent } from 'store/hooks';
import { IMinisterModel } from 'store/hooks/subscriber/interfaces/IMinisterModel';
import { useMinisters } from 'store/hooks/subscriber/useMinisters';
import {
  Checkbox,
  FlexboxTable,
  generateQuery,
  IContentModel,
  ITableInternalRow,
  Row,
} from 'tno-core';

import { determineColumns } from './constants';
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
  const navigate = useNavigate();

  const [selected, setSelected] = React.useState<IContentSearchResult[]>([]);
  const [filteredContent, setFilteredContent] = React.useState<IContentSearchResult[]>([]);
  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [ministers, setMinisters] = React.useState<IMinisterModel[]>([]);
  const [userMinisters, setUserMinisters] = React.useState<IMinisterModel[]>([]);
  const [loading, setLoading] = React.useState(false);

  const selectedIds = selected.map((i) => i.id.toString());

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
        if (!loading) {
          setLoading(true);
          const res = await findContentWithElasticsearch(filter, false);
          const content = res.hits.hits.map((r) => {
            const content = r._source as IContentModel;
            const result = castToSearchResult(content);
            return result;
          });
          return content;
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
          if (includeMention && m.name !== ministerName) {
            mentions.push(!!aliases ? aliases[0] : m.name);
          }
        });
      return mentions;
    };

    const displayData = async () => {
      const contentList: IContentSearchResult[] = [];
      if (userInfo?.preferences?.myMinisters?.length > 0 && ministers.length > 0) {
        let ministerModels = ministers
          .filter((m) => userInfo?.preferences?.myMinisters?.includes(m.id))
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
                  defaultSearchOperator: 'or',
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
          setContent(contentList);
          setUserMinisters(ministerModels);
        }
      }
    };
    displayData().catch(() => {});
  }, [userInfo?.preferences?.myMinisters, ministers, fetchResults, filter]);

  React.useEffect(() => {
    setFilteredContent(content);
  }, [content]);

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentSearchResult>) => {
    if (row.isSelected) {
      setSelected(
        row.table.rows.filter((r) => r.isSelected).map((r) => castToSearchResult(r.original)),
      );
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  const hideGroup = (minister: IMinisterModel, hide: boolean) => {
    const ministersList = [...userMinisters];
    const m = ministersList.find((m) => m.id === minister.id);
    if (m) {
      m.hide = hide;
    }
    setUserMinisters(ministersList);
    let filteredContent = [...content];
    ministersList
      .filter((m) => m.hide)
      .forEach((m) => {
        filteredContent = filteredContent.filter((c) => c.ministerName !== m.name);
      });
    setFilteredContent(filteredContent);
  };

  return (
    <styled.MyMinister>
      <ContentListActionBar
        content={selected}
        onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
      />
      <DateFilter filter={filter} storeFilter={storeFilter} />
      <div className="ministerCheckboxes">
        <span className="option">SHOW:</span>
        {userMinisters.map((m) => {
          return (
            <Checkbox
              key={m.id}
              label={`${m.name} (${m.contentCount})`}
              className="option"
              checked={!m.hide}
              onChange={(e) => {
                hideGroup(m, !(e.target as HTMLInputElement).checked);
              }}
            />
          );
        })}
      </div>
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns('all')}
          onSelectedChanged={handleSelectedRowsChanged}
          selectedRowIds={selectedIds}
          isMulti
          groupBy={(item) => item.original.ministerName ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/my-minister/${e.original.id}`);
          }}
          data={filteredContent}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.MyMinister>
  );
};
