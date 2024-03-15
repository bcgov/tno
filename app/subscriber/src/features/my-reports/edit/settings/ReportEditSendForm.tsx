import { Col, FormikCheckbox, FormikText, Row } from 'tno-core';

import { ReportSchedule } from './components';

export const ReportEditSendForm = () => {
  return (
    <div>
      <Row alignItems="center">
        <Col flex="1">
          <FormikText name="settings.subject.text" label="Email subject line:" required />
        </Col>
        <FormikCheckbox name="settings.subject.showTodaysDate" label="Append today's date" />
      </Row>
      <Col className="frm-in">
        <label>Recipients:</label>
      </Col>
      <Col className="frm-in schedules">
        <label>Schedules:</label>
        <Row gap="1rem">
          <Col flex="1">
            <ReportSchedule index={0} label="Schedule 1" />
          </Col>
          <Col flex="1">
            <ReportSchedule index={1} label="Schedule 2" />
          </Col>
        </Row>
      </Col>
    </div>
  );
};
