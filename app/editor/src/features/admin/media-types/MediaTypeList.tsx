import { FormPage, IconButton } from 'components/form';
import { IMediaTypeModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useMediaTypes } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, GridTable, Row } from 'tno-core';

import { columns } from './constants';
import { MediaTypeFilter } from './MediaTypeFilter';
import * as styled from './styled';

export const MediaTypeList: React.FC = () => {
  const navigate = useNavigate();
  const [{ mediaTypes }, api] = useMediaTypes();
  const [{ requests }] = useApp();

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
            Media types provide a way to identify the media that the content represents. They are
            integral to ingestion services. Do not edit unless the ingestion services are also
            updated.
          </Col>
          <IconButton
            iconType="plus"
            label="Add New Media Type"
            onClick={() => navigate('/admin/media/types/0')}
          />
        </Row>
        <GridTable
          columns={columns}
          header={MediaTypeFilter}
          manualPageSize
          isLoading={!!requests.length}
          data={items}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></GridTable>
      </FormPage>
    </styled.MediaTypeList>
  );
};
