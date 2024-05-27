import { FormPage } from 'components/formpage';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSettings } from 'store/hooks/admin';
import { Col, FlexboxTable, IconButton, ISettingModel, Row } from 'tno-core';

import { columns } from './constants';
import { SettingFilter } from './SettingFilter';
import * as styled from './styled';

const SettingList: React.FC = () => {
  const navigate = useNavigate();
  const [{ settings }, api] = useSettings();

  const [items, setItems] = React.useState<ISettingModel[]>([]);

  React.useEffect(() => {
    if (!settings.length) {
      api.findAllSettings().then((data) => {
        setItems(data);
      });
    } else {
      setItems(settings);
    }
  }, [api, settings]);

  return (
    <styled.SettingList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">
            Settings provide a way for system administrators to configure the solution. Do not
            change these unless you are aware of what each setting does.
          </Col>
          <IconButton
            iconType="plus"
            label={`Add new setting`}
            onClick={() => navigate(`/admin/settings/0`)}
          />
        </Row>
        <SettingFilter
          onFilterChange={(filter) => {
            if (filter && filter.length) {
              const value = filter.toLocaleLowerCase();
              setItems(
                settings.filter(
                  (i) =>
                    i.name.toLocaleLowerCase().includes(value) ||
                    i.description.toLocaleLowerCase().includes(value),
                ),
              );
            } else {
              setItems(settings);
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
    </styled.SettingList>
  );
};

export default SettingList;
