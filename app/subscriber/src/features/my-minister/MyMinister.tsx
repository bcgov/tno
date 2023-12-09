import { MsearchMultisearchBody } from '@elastic/elasticsearch/lib/api/types';
import { FolderSubMenu } from 'components/folder-sub-menu';
import { ContentActionBar } from 'components/tool-bar';
import { determineColumns } from 'features/home/constants';
import { filterFormat } from 'features/search-page/utils';
import { castToSearchResult } from 'features/utils';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp, useContent, useLookup } from 'store/hooks';
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
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [homeItems, setHomeItems] = React.useState<IContentModel[]>([]);
  const [ministerNames, setMinisterNames] = React.useState<string[]>([]);
  const [ministers, setMinisters] = React.useState<IMinisterModel[]>([]);
  const [loading, setLoading] = React.useState(false);
  const [{ actions }] = useLookup();

  //  convert minister name to alias (e.g. David Eby -> D. Eby OR David Eby)
  const toMinisterAlias = (ministerName: string) => {
    const firstInitial = ministerName.charAt(0);
    const lastName = ministerName.split(' ')[1];
    return `"${firstInitial}. ${lastName}" "${ministerName}"`;
  };

  React.useEffect(() => {
    if (!ministers.length) {
      api.getMinisters().then((data) => {
        setMinisters(data);
      });
    }
  }, [api, ministers.length]);

  /** controls the checking and unchecking of rows in the list view */
  const handleSelectedRowsChanged = (row: ITableInternalRow<IContentModel>) => {
    if (row.isSelected) {
      setSelected(row.table.rows.filter((r) => r.isSelected).map((r) => r.original));
    } else {
      setSelected((selected) => selected.filter((r) => r.id !== row.original.id));
    }
  };

  const fetchResults = React.useCallback(
    async (filter: MsearchMultisearchBody) => {
      try {
        if (!loading) {
          setLoading(true);
          const res = await findContentWithElasticsearch(filter, false);
          setHomeItems(
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
    if (!!filter.search && ministerNames.length > 0) {
      fetchResults(
        generateQuery(
          filterFormat(
            {
              ...filter,
              inByline: true,
              inHeadline: true,
              inStory: true,
              defaultSearchOperator: 'or',
            },
            actions,
          ),
        ),
      );
    }
    // only want the effect to trigger when aliases is populated, not every time the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, ministerNames]);

  React.useEffect(() => {
    if (userInfo?.preferences?.myMinisters?.length > 0 && ministers.length > 0) {
      let selectedMinisters: string[] = [];
      selectedMinisters = ministers
        .filter((m) => userInfo?.preferences?.myMinisters?.includes(m.id))
        .map((x) => x.name);
      setMinisterNames(selectedMinisters);
    }
  }, [ministers, userInfo?.preferences?.myMinisters, actions]);

  /** retrigger content fetch when change is applied */
  React.useEffect(() => {
    if (ministerNames.length > 0) {
      storeMyMinisterFilter({ ...filter, search: toMinisterAlias(ministerNames.toString()) });
    }
    // only want the effect to trigger when aliases is populated, not every time the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ministerNames]);
  return (
    <styled.MyMinister>
      <ContentActionBar content={selected} onList />
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determineColumns('all')}
          onSelectedChanged={handleSelectedRowsChanged}
          isMulti
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/my-minister/${e.original.id}`);
          }}
          data={homeItems}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.MyMinister>
  );
};
