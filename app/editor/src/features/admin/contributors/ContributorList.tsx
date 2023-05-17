import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useContributors } from 'store/hooks/admin';
import { Col, FlexboxTable, FormPage, IconButton, IContributorModel, Row } from 'tno-core';

import { columns } from './constants';
import * as styled from './styled';

export const ContributorList: React.FC = () => {
  const navigate = useNavigate();
  const [{ contributors }, api] = useContributors();

  const [items, setItems] = React.useState<IContributorModel[]>([]);

  React.useEffect(() => {
    if (!contributors.length) {
      api.findAllContributor().then((data) => {
        setItems(data);
      });
    } else {
      setItems(contributors);
    }
  }, [api, contributors]);

  return (
    <styled.ContributorList>
      <FormPage>
        <Row className="add-media" justifyContent="flex-end">
          <Col flex="1 1 0">Provides a way to manage columnist and pundit list.</Col>
          <IconButton
            iconType="plus"
            label={`Add new columnist/pundit`}
            onClick={() => navigate(`/admin/contributors/0`)}
          />
        </Row>
        <FlexboxTable
          rowId="id"
          data={items}
          columns={columns}
          showSort={true}
          onRowClick={(row) => navigate(`${row.original.id}`)}
        />
      </FormPage>
    </styled.ContributorList>
  );
};
