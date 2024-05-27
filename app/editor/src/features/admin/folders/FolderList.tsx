import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useFolders } from 'store/hooks/admin';
import { useAdminStore } from 'store/slices';
import { Col, FlexboxTable, IconButton, IFolderModel, Row } from 'tno-core';

import { columns } from './constants';
import { FolderFilter } from './FolderFilter';
import * as styled from './styled';

export const FolderList: React.FC = () => {
  const navigate = useNavigate();
  const [{ initialized, folders }, { findAllFolders }] = useFolders();
  const [{ folderFilter }] = useAdminStore();

  const [items, setItems] = React.useState<IFolderModel[]>(folders);

  React.useEffect(() => {
    if (!initialized) {
      findAllFolders().catch(() => {});
    }
    // The api will cause a double render because findAllFolders(...) updates the store.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initialized]);

  React.useEffect(() => {
    if (folderFilter && folderFilter.length) {
      const value = folderFilter.toLocaleLowerCase();
      setItems(
        folders.filter(
          (i) =>
            i.name.toLocaleLowerCase().includes(value) ||
            i.description.toLocaleLowerCase().includes(value) ||
            i.owner?.username.toLocaleLowerCase().includes(value) ||
            i.owner?.displayName.toLocaleLowerCase().includes(value) ||
            i.owner?.firstName.toLocaleLowerCase().includes(value) ||
            i.owner?.lastName.toLocaleLowerCase().includes(value),
        ),
      );
    } else {
      setItems(folders);
    }
  }, [folders, folderFilter]);

  return (
    <styled.FolderList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">A folder provides a way to collect content for reports.</Col>
          <IconButton
            iconType="plus"
            label={`Add new folder`}
            onClick={() => navigate(`/admin/folders/0`)}
          />
        </Row>
        <FolderFilter />
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          showSort={true}
          onRowClick={(row) => navigate(`${row.original.id}`)}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.FolderList>
  );
};

export default FolderList;
