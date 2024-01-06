import { useFormikContext } from 'formik';
import { Col, Row } from 'tno-core';

import { IReportForm } from '../interfaces';

export const ReportSubscribers: React.FC = () => {
  const { values } = useFormikContext<IReportForm>();

  return (
    <Col className="subscribers">
      <Row className="header">
        <Col flex="1">Username</Col>
        <Col flex="1">LastName</Col>
        <Col flex="1">FirstName</Col>
        <Col flex="2">Email</Col>
        <Col flex="1">Format</Col>
      </Row>
      {values.subscribers.map((sub) => {
        return (
          <Row key={sub.id} className="row">
            <Col flex="1">{sub.username}</Col>
            <Col flex="1">{sub.lastName}</Col>
            <Col flex="1">{sub.firstName}</Col>
            <Col flex="2">{sub.email}</Col>
            <Col flex="1">{sub.format}</Col>
          </Row>
        );
      })}
    </Col>
  );
};
