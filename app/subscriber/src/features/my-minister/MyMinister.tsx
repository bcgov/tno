import { defaultUser } from 'features/access-request/constants';
import {
  IContentListAdvancedFilter,
  IContentListFilter,
} from 'features/content/list-view/interfaces';
import { columns } from 'features/home/constants';
import { makeFilter } from 'features/home/utils';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp, useContent, useUsers } from 'store/hooks';
import { FlexboxTable, IContentModel, IUserModel, Page, Row } from 'tno-core';

import * as styled from './styled';

export const MyMinister: React.FC = () => {
  const [{ filter, filterAdvanced }, { findContent }] = useContent();
  const [homeItems, setHomeItems] = React.useState<IContentModel[]>([]);
  const navigate = useNavigate();
  const [user, setUser] = React.useState<IUserModel>(defaultUser);
  const [{ userInfo }] = useApp();
  const api = useUsers();

  React.useEffect(() => {
    if (userInfo && userInfo.id) {
      api.getUser(userInfo.id).then((data) => {
        setUser(data);
      });
    }
  }, [userInfo, api]);

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
    fetch({ ...filter, ...filterAdvanced, keyword: user.preferences?.myMinister ?? '' });
  }, [filter, filterAdvanced, fetch, user]);
  return (
    <styled.MyMinister>
      <Row className="table-container">
        <FlexboxTable
          rowId="id"
          columns={columns}
          isMulti
          groupBy={(item) => item.original.source?.name ?? ''}
          onRowClick={(e: any) => {
            navigate(`/view/${e.original.id}`);
          }}
          data={homeItems || []}
          pageButtons={5}
          showPaging={false}
        />
      </Row>
    </styled.MyMinister>
  );
};
