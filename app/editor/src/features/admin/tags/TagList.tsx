import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTags } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, ITagModel, Row } from 'tno-core';

import { columns } from './constants';
import * as styled from './styled';
import { TagFilter } from './TagFilter';

const TagList: React.FC = () => {
  const navigate = useNavigate();
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
            onClick={() => navigate(`/admin/tags/0`)}
          />
        </Row>
        <TagFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                tags.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value) ||
                    i.code.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(tags);
            }
          }}
        />
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          showSort={true}
          onRowClick={(row) => navigate(`${row.original.id}`)}
          pagingEnabled={false}
        />
      </FormPage>
    </styled.TagList>
  );
};

export default TagList;
