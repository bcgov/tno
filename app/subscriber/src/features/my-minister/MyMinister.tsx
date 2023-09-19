import { FolderSubMenu } from 'components/folder-sub-menu';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { determineColumns } from 'features/home/constants';
import { makeFilter } from 'features/home/utils';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp, useContent } from 'store/hooks';
import { IMinisterModel } from 'store/hooks/subscriber/interfaces/IMinisterModel';
import { useMinisters } from 'store/hooks/subscriber/useMinisters';
import { FlexboxTable, IContentModel, ITableInternalRow, Page, Row } from 'tno-core';

import * as styled from './styled';

export const MyMinister: React.FC = () => {
  const [{ filter, filterAdvanced }, { findContent }] = useContent();
  const [{ userInfo }] = useApp();
  const [, api] = useMinisters();
  const navigate = useNavigate();
  const [selected, setSelected] = React.useState<IContentModel[]>([]);
  const [homeItems, setHomeItems] = React.useState<IContentModel[]>([]);
  const [ministerNames, setMinisterNames] = React.useState<string[]>([]);
  const [ministers, setMinisters] = React.useState<IMinisterModel[]>([]);
  const [, setLoading] = React.useState(false);

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

  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setLoading(true);
        const data = await findContent(
          makeFilter({
            ...filter,
            startDate: '',
            contentTypes: [],
            endDate: '',
          }),
        );
        // don't want to keyword fetch when there is nothing to fetch
        return new Page(data.page - 1, data.quantity, data?.items, data.total);
      } catch (error) {
        // TODO: Handle error
        throw error;
      } finally {
        setLoading(false);
      }
    },
    [findContent],
  );

  React.useEffect(() => {
    if (userInfo?.preferences?.myMinisters?.length > 0 && ministers.length > 0) {
      let selectedMinisters: string[] = [];
      selectedMinisters = ministers
        .filter((m) => userInfo?.preferences?.myMinisters?.includes(m.id))
        .map((x) => x.name);
      setMinisterNames(selectedMinisters);
    }
  }, [ministers, userInfo?.preferences?.myMinisters]);

  /** retrigger content fetch when change is applied */
  React.useEffect(() => {
    if (!ministerNames.length) return;
    fetch({
      ...filter,
      ...filterAdvanced,
      names: ministerNames.toString(),
    }).then((data) => {
      setHomeItems(data.items);
    });
    // only want the effect to trigger when aliases is populated, not every time the filter changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ministerNames]);
  return (
    <styled.MyMinister>
      <FolderSubMenu selectedContent={selected} />
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
