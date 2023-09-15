import { useFormikContext } from 'formik';
import React from 'react';
import {
  Col,
  FieldSize,
  FormikSelect,
  getEnumStringOptions,
  IIngestModel,
  Row,
  ScheduleTypeName,
  Show,
} from 'tno-core';

import { ScheduleAdvanced, ScheduleContinuous, ScheduleDaily } from '.';
import * as styled from './styled';

interface IIngestScheduleProps {}

const IngestSchedule: React.FC<IIngestScheduleProps> = () => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();

  const scheduleTypeOptions = getEnumStringOptions(ScheduleTypeName);

  return (
    <styled.IngestSchedule className="schedule">
      <h2>{values.name}</h2>
      <p>
        A service schedule provides a way to manage when and how often source content is ingested.
      </p>
      <Row colGap="1em" nowrap>
        <Col>
          <FormikSelect
            label="Schedule Type"
            name="scheduleType"
            tooltip="Choose how the service will be run"
            isClearable={false}
            options={scheduleTypeOptions}
            value={scheduleTypeOptions.find((o) => o.value === values.scheduleType)}
            width={FieldSize.Medium}
            onChange={(newValue: any) => {
              if (values.scheduleType !== newValue.value) {
                // Clear values when type changes.
                setFieldValue('schedules', []);
              }
            }}
          />
        </Col>
      </Row>
      <hr />
      <Show visible={values.scheduleType === ScheduleTypeName.Daily}>
        <ScheduleDaily index={0} />
      </Show>
      <Show visible={values.scheduleType === ScheduleTypeName.Continuous}>
        <ScheduleContinuous />
      </Show>
      <Show visible={values.scheduleType === ScheduleTypeName.Advanced}>
        <ScheduleAdvanced />
      </Show>
    </styled.IngestSchedule>
  );
};

export default IngestSchedule;
