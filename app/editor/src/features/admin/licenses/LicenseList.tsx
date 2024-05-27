import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useLicenses } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, ILicenseModel, Row } from 'tno-core';

import { columns } from './constants';
import { LicenseFilter } from './LicenseFilter';
import * as styled from './styled';

const LicenseList: React.FC = () => {
  const navigate = useNavigate();
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
        <LicenseFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                licenses.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value) ||
                    `${i.ttl}`.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(licenses);
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
    </styled.LicenseList>
  );
};
export default LicenseList;
