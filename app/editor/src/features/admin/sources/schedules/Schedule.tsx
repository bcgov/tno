import { FieldSize } from 'components/form';
import { FormikSelect } from 'components/formik';
import { useFormikContext } from 'formik';
import { DataSourceScheduleTypeName, IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import { ScheduleAdvanced, ScheduleContinuous, ScheduleDaily } from '.';
import { scheduleTypeOptions } from './constants';
import * as styled from './styled';

interface IScheduleProps {}

export const Schedule: React.FC<IScheduleProps> = () => {
  const { values } = useFormikContext<IDataSourceModel>();

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
      <FormikSelect
        label="Schedule Type"
        name="scheduleType"
        options={scheduleTypeOptions}
        width={FieldSize.Medium}
      />
      {form(values.scheduleType)}
    </styled.Schedule>
  );
};
