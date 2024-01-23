import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { ContentListActionBar } from 'components/tool-bar';
import { determineColumns } from 'features/home/constants';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import { IContentSearchResult } from 'features/utils/interfaces';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp, useContent } from 'store/hooks';
import { IMinisterModel } from 'store/hooks/subscriber/interfaces/IMinisterModel';
import { useMinisters } from 'store/hooks/subscriber/useMinisters';
import { FlexboxTable, generateQuery, IContentModel, ITableInternalRow, Row } from 'tno-core';

import * as styled from './styled';

export const MyMinister: React.FC = () => {
  const [
    {
      myMinister: { filter },
    },
    { findContentWithElasticsearch, storeMyMinisterFilter },
  ] = useContent();
  const [{ userInfo }] = useApp();
  const [, api] = useMinisters();
  const navigate = useNavigate();

  const [selected, setSelected] = React.useState<IContentSearchResult[]>([]);
  const [content, setContent] = React.useState<IContentSearchResult[]>([]);
  const [ministers, setMinisters] = React.useState<IMinisterModel[]>([]);
  const [loading, setLoading] = React.useState(false);

  const selectedIds = selected.map((i) => i.id.toString());

  //  convert minister name to alias (e.g. David Eby -> D. Eby OR David Eby)
  const getAliasFromMinisterName = (ministerName: string) => {
    const firstInitial = ministerName.charAt(0);
    const lastName = ministerName.split(' ')[1];
    return `${firstInitial}. ${lastName}`;
  };

  const makeSimpleQueryString = (terms: string[]) => {
    if (terms.length === 1) return `"${terms[0]}"`;
    const allButLastTerm = terms.slice(0, terms.length - 1);
    const lastTerm = terms[terms.length - 1];
    return allButLastTerm.map((t) => `"${t}"`).join(' | ') + ` | "${lastTerm}"`;
  };

  React.useEffect(
    () => {
      if (!ministers.length) {
        api.getMinisters().then((data) => {
          setMinisters(data);
        });
      }
    },
    // do not want to trigger on loading change, will cause infinite loop
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
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
    if (!!filter.search) {
      fetchResults(
        generateQuery(
          filterFormat({
            ...filter,
            inByline: true,
            inHeadline: true,
            inStory: true,
            defaultSearchOperator: 'or',
          }),
        ),
      );
    }
    // only want the effect to trigger when aliases is populated, not every time the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter.search]);

  React.useEffect(() => {
    var searchFilterTerms: string[] = [];
    if (userInfo?.preferences?.myMinisters?.length > 0 && ministers.length > 0) {
      let ministerModels = ministers.filter((m) =>
        userInfo?.preferences?.myMinisters?.includes(m.id),
      );
      if (ministerModels) {
        ministerModels.forEach((m) => {
          searchFilterTerms.push(m.name);
          let ministerAliases = !m.aliases.length
            ? [getAliasFromMinisterName(m.name)]
            : m.aliases.split(',').map(function (item) {
                return item.trim();
              });
          ministerAliases.forEach((m) => searchFilterTerms.push(m));
        });
        // only store a search filter if the user has selected at least one "My Minister"
        storeMyMinisterFilter({ ...filter, search: makeSimpleQueryString(searchFilterTerms) });
      }
    }
  }, [userInfo?.preferences?.myMinisters, ministers, filter, storeMyMinisterFilter]);

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

  return (
    <styled.MyMinister>
      <ContentListActionBar
        content={selected}
        onSelectAll={(e) => (e.target.checked ? setSelected(content) : setSelected([]))}
      />
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns('all')}
          onSelectedChanged={handleSelectedRowsChanged}
          selectedRowIds={selectedIds}
          isMulti
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/my-minister/${e.original.id}`);
          }}
          data={content}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.MyMinister>
  );
};
