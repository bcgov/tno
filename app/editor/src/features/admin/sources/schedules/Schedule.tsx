import { Tab, Tabs } from 'components/tabs';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { Outlet } from 'react-router-dom';

import { defaultSource } from '../constants';
import * as styled from '../styled';

interface IScheduleProps {
  values?: IDataSourceModel;
}

export const Schedule: React.FC<IScheduleProps> = ({ values = defaultSource }) => {
  return (
    <styled.Schedule className="schedule">
      <h2>Schedule</h2>

      <Tabs
        tabs={
          <>
            <Tab exact navigateTo="schedules" label="Continuos" />
            <Tab exact navigateTo="schedules/daily" label="Daily" />
            <Tab exact navigateTo="schedules/advanced" label="Advanced" />
          </>
        }
      >
        <Outlet />
      </Tabs>
    </styled.Schedule>
  );
};
