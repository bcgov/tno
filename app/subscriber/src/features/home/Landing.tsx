import { MenuItemNames } from 'components/layout/constants/MenuItemNames';
import { FaAngleLeft, FaAngleRight, FaCalendarAlt, FaUserCircle } from 'react-icons/fa';
import { useParams } from 'react-router-dom';
import { Button, Col, FieldSize, GridTable, Row, Show, Text, useKeycloakWrapper } from 'tno-core';

import * as styled from './styled';

export const Landing: React.FC = () => {
  const { id } = useParams();
  const keycloak = useKeycloakWrapper();

  return (
    <styled.Landing>
      <Col className="main-panel">
        <div className="title">{MenuItemNames[id?.toString() as keyof typeof MenuItemNames]}</div>
        <div className="content">
          <Show visible={id === MenuItemNames.Home}>
            <Row>
              <div className="show-media-label">SHOW MEDIA TYPE:</div>
              <Row className="filter-buttons">
                <Button className="active">ALL</Button>
                <Button className="inactive">RADIO/TV</Button>
                <Button className="inactive">INTERNET</Button>
                <Button className="inactive">CP NEWS</Button>
              </Row>
            </Row>
            <Row justifyContent="center" className="date-navigator">
              <FaAngleLeft />
              DATE WILL GO HERE
              <FaAngleRight />
              <FaCalendarAlt className="calendar" />
            </Row>
            <Row>
              <GridTable
                data={[]}
                columns={[
                  {
                    id: 'code',
                    Header: 'Tone',
                  },
                  {
                    id: 'name',
                    Header: 'Headline',
                  },
                  {
                    id: 'description',
                    Header: 'Section/Page',
                  },
                ]}
              />
            </Row>
          </Show>
        </div>
      </Col>
      <Col className="right-panel">
        <Row className="header">
          <Text className="search" width={FieldSize.Big} name="search" />
          <div onClick={() => keycloak.instance.logout()} className="logout">
            <FaUserCircle />
            Logout
          </div>
        </Row>
        <div className="commentary">
          <div className="title">Commentary</div>
          <div className="content"></div>
        </div>
        <div className="commentary">
          <div className="title">Front Pages</div>
          <div className="content"></div>
        </div>
      </Col>
    </styled.Landing>
  );
};
