import { FormPage, IconButton } from 'components/form';
import { ITagModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTags } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, GridTable, Row } from 'tno-core';

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
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">Tags provide a way to identify content.</Col>
          <IconButton
            iconType="plus"
            label={`Add new tag`}
            onClick={() => navigate(`/admin/tags/NEW`)}
          />
        </Row>
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
