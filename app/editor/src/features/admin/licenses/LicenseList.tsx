import { ILicenseModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useLicenses } from 'store/hooks/admin';
import { useApp } from 'store/hooks/app/useApp';
import { Col, FormPage, GridTable, IconButton, Row } from 'tno-core';

import { columns } from './constants';
import { LicenseListFilter } from './LicenseListFilter';
import * as styled from './styled';

export const LicenseList: React.FC = () => {
  const navigate = useNavigate();
  const [{ requests }] = useApp();
  const [{ licenses }, api] = useLicenses();

  const [items, setItems] = React.useState<ILicenseModel[]>([]);

  React.useEffect(() => {
    if (!licenses.length) {
      api.findAllLicenses().then((data) => {
        setItems(data);
      });
    } else {
      setItems(licenses);
    }
  }, [api, licenses]);

  return (
    <styled.LicenseList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Licences provide a way to group related content and identify content for reports.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new licence`}
            onClick={() => navigate(`/admin/licences/0`)}
          />
        </Row>
        <GridTable
          columns={columns}
          header={LicenseListFilter}
          manualPageSize
          isLoading={!!requests.length}
          data={items}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        ></GridTable>
      </FormPage>
    </styled.LicenseList>
  );
};
