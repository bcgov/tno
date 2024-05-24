import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useMediaTypes } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, IMediaTypeModel, Row } from 'tno-core';

import { columns } from './constants';
import { MediaTypeFilter } from './MediaTypeFilter';
import * as styled from './styled';

const MediaTypeList: React.FC = () => {
  const navigate = useNavigate();
  const [{ mediaTypes }, api] = useMediaTypes();

  const [items, setItems] = React.useState<IMediaTypeModel[]>([]);

  React.useEffect(() => {
    if (!mediaTypes.length) {
      api.findAllMediaTypes().then((data) => {
        setItems(data);
      });
    } else {
      setItems(mediaTypes);
    }
  }, [api, mediaTypes]);

  return (
    <styled.MediaTypeList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Media Types provide a way to designate how and where content is displayed to
            subscribers.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new media type`}
            onClick={() => navigate(`/admin/media-types/0`)}
          />
        </Row>
        <MediaTypeFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                mediaTypes.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(mediaTypes);
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
    </styled.MediaTypeList>
  );
};

export default MediaTypeList;
