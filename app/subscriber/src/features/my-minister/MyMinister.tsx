import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { determinecolumns } from 'features/home/constants';
import { makeFilter } from 'features/home/utils';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContent } from 'store/hooks';
import { FlexboxTable, IContentModel, Page, Row } from 'tno-core';

import * as styled from './styled';

export const MyMinister: React.FC = () => {
  const [{ filter, filterAdvanced }, { findContent }] = useContent();
  const [homeItems, setHomeItems] = React.useState<IContentModel[]>([]);
  const navigate = useNavigate();

  const [, setLoading] = React.useState(false);
  const fetch = React.useCallback(
    async (filter: IContentListFilter & Partial<IContentListAdvancedFilter>) => {
      try {
        setLoading(true);
        const data = await findContent(
          makeFilter({
            ...filter,
            startDate: '',
            endDate: '',
          }),
        );
        setHomeItems(data.items);
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

  /** retrigger content fetch when change is applied */
  React.useEffect(() => {
    fetch({
      ...filter,
      ...filterAdvanced,
      keyword: localStorage.getItem('myMinister') ?? '',
    });
  }, [filter, filterAdvanced, fetch]);
  return (
    <styled.MyMinister>
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={determinecolumns('all')}
          isMulti
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/my-minister/${e.original.id}`);
          }}
          data={homeItems || []}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.MyMinister>
  );
};
