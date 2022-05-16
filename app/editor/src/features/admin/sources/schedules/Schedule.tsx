import { FieldSize } from 'components/form';
import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { DataSourceScheduleTypeName, IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { Col, Row } from 'tno-core';

import { Connection } from '../media-types';
import { ScheduleAdvanced, ScheduleContinuous, ScheduleDaily } from '.';
import { scheduleTypeOptions } from './constants';
import * as styled from './styled';

interface IScheduleProps {}

export const Schedule: React.FC<IScheduleProps> = () => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();

  const form = (scheduleType: DataSourceScheduleTypeName) => {
    switch (scheduleType) {
      case DataSourceScheduleTypeName.Continuous:
        return <ScheduleContinuous index={0} />;
      case DataSourceScheduleTypeName.Daily:
        return <ScheduleDaily index={0} />;
      case DataSourceScheduleTypeName.Advanced:
        return <ScheduleAdvanced />;
    }
  };

  return (
    <styled.Schedule className="schedule">
      <p>
        A service schedule provides a way to manage when and how often source content is imported.
      </p>
      <Row colGap="1em" nowrap>
        <Col>
          <FormikSelect
            label="Schedule Type"
            name="scheduleType"
            options={scheduleTypeOptions}
            width={FieldSize.Medium}
            onChange={(newValue: any) => {
              if (
                newValue.value === DataSourceScheduleTypeName.None &&
                values.schedules.some((s) => s.id === 0)
              ) {
                setFieldValue(
                  'schedules',
                  values.schedules.filter((s) => s.id !== 0),
                );
              } else if (!values.topic) {
                // Default value to unique code, but allow override.
                setFieldValue('topic', values.code);
              }
            }}
          />
          <FormikText
            label="Kafka Topic"
            name="topic"
            width={FieldSize.Medium}
            disabled={values.scheduleType === DataSourceScheduleTypeName.None}
            required={values.scheduleType !== DataSourceScheduleTypeName.None}
          />
        </Col>
        <Col flex="1 1 50%">
          <p>
            A Kafka Topic is a category/feed name to which records are stored and published. If this
            data-source has a running service, the content will be ingested and placed in the Kafka
            Event Streaming data storage location.
          </p>
          <p>
            The topic should be unique, or all content stored within it should be the same format.
          </p>
        </Col>
        <Col>
          <Connection />
        </Col>
      </Row>
      <hr />
      {form(values.scheduleType)}
    </styled.Schedule>
  );
};
