import React from 'react';
import { Outlet, useNavigate, useParams } from 'react-router-dom';
import { Col, IconButton, Row, Tab, Tabs } from 'tno-core';

import * as styled from './styled';

/** The page used to view and edit series the administrative section. */
const SeriesForm: React.FC = () => {
  const { id } = useParams();
  const navigate = useNavigate();

  const targetSeriesId = Number(id);

  return (
    <styled.SeriesForm>
      <Row>
        <Col flex="2 1">
          <IconButton
            iconType="back"
            label="Back to Show/Programs"
            className="back-button"
            onClick={() => navigate('/admin/programs')}
          />
          <Tabs
            tabs={
              <>
                <Tab navigateTo="details" label="Details" exact activePaths={[`${id}`]} />
                <Tab navigateTo="merge" label="Merge" disabled={targetSeriesId === 0} />
              </>
            }
          >
            <Outlet />
          </Tabs>
        </Col>
      </Row>
    </styled.SeriesForm>
  );
};

export default SeriesForm;
