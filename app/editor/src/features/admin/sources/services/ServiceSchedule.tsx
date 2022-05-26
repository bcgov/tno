import { FieldSize } from 'components/form';
import { FormikSelect } from 'components/formik';
import { useFormikContext } from 'formik';
import { DataSourceScheduleTypeName, IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import ReactTooltip from 'react-tooltip';
import { useLookup } from 'store/hooks';
import { Col, Row } from 'tno-core';

import { ScheduleAdvanced, ScheduleContinuous, ScheduleDaily } from '.';
import { scheduleTypeOptions } from './constants';
import * as styled from './styled';

interface IServiceScheduleProps {}

export const ServiceSchedule: React.FC<IServiceScheduleProps> = () => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const [lookups] = useLookup();

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const owner = lookups.users.find((mt) => mt.id === values.ownerId);
    setFieldValue('owner', owner);
  }, [lookups.users, setFieldValue, values.ownerId, values.owner]);

  React.useEffect(() => {
    ReactTooltip.rebuild();
  });

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
    <styled.ServiceSchedule className="schedule">
      <p>
        A service schedule provides a way to manage when and how often source content is imported.
      </p>
      <Row colGap="1em" nowrap>
        <Col>
          <FormikSelect
            label="Schedule Type"
            name="scheduleType"
            tooltip="Choose how the service will be run"
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
        </Col>
      </Row>
      <hr />
      {form(values.scheduleType)}
    </styled.ServiceSchedule>
  );
};
