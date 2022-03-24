import { Col, Row } from 'components/flex';
import { FieldSize } from 'components/form';
import { FormikCheckbox, FormikText } from 'components/formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import { defaultSource } from '../constants';
import * as styled from './styled';

interface IScheduleContinuosProps {
  values?: IDataSourceModel;
}

export const ScheduleContinuos: React.FC<IScheduleContinuosProps> = ({
  values = defaultSource,
}) => {
  return (
    <styled.Schedule className="schedule">
      <Row>
        Run service every
        <FormikText name="time" type="number" width={FieldSize.Tiny} />
        on the following days;
      </Row>
      <Col>
        <FormikCheckbox label="Monday" name="monday" />
        <FormikCheckbox label="Tuesday" name="monday" />
        <FormikCheckbox label="Wednesday" name="monday" />
        <FormikCheckbox label="Thursday" name="monday" />
        <FormikCheckbox label="Friday" name="monday" />
        <FormikCheckbox label="Saturday" name="monday" />
        <FormikCheckbox label="Sunday" name="monday" />
      </Col>
    </styled.Schedule>
  );
};
