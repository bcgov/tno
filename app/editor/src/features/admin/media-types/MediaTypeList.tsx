import { FormPage } from 'components/form/formpage/styled';
import { IMediaTypeModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useMediaTypes } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { GridTable } from 'tno-core';

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
