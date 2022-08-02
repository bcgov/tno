import { FormPage } from 'components/form/formpage/styled';
import { ITagModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTags } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { GridTable } from 'tno-core';

import { columns } from './constants';
import * as styled from './styled';
import { TagListFilter } from './TagListFilter';

export const TagList: React.FC = () => {
  const navigate = useNavigate();
  const [{ requests }] = useApp();
  const [{ tags }, api] = useTags();

  const [items, setItems] = React.useState<ITagModel[]>([]);

  React.useEffect(() => {
    if (!tags.length) {
      api.findAllTags().then((data) => {
        setItems(data);
      });
    } else {
      setItems(tags);
    }
  }, [api, tags]);

  return (
    <styled.TagList>
      <FormPage>
        <GridTable
          columns={columns}
          header={TagListFilter}
          manualPageSize
          isLoading={!!requests.length}
          data={items}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></GridTable>
      </FormPage>
    </styled.TagList>
  );
};
