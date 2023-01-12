import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel, ScheduleTypeName } from 'hooks/api-editor';
import React from 'react';
import { Col, FieldSize, FormikSelect, Row, Show } from 'tno-core';
import { getEnumStringOptions } from 'utils';

import { ScheduleAdvanced, ScheduleContinuous, ScheduleDaily } from '.';
import * as styled from './styled';

interface IIngestScheduleProps {}

export const IngestSchedule: React.FC<IIngestScheduleProps> = () => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  useTooltips();

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
            options={scheduleTypeOptions}
            value={scheduleTypeOptions.find((o) => o.value === values.scheduleType)}
            width={FieldSize.Medium}
            onChange={(newValue: any) => {
              if (
                newValue.value === ScheduleTypeName.None &&
                values.schedules.some((s) => s.id === 0)
              ) {
                setFieldValue(
                  'schedules',
                  values.schedules.filter((s) => s.id !== 0),
                );
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
        <ScheduleContinuous index={0} />
      </Show>
      <Show visible={values.scheduleType === ScheduleTypeName.Advanced}>
        <ScheduleAdvanced />
      </Show>
    </styled.IngestSchedule>
  );
};
