import { Tab, Tabs } from 'components/tabs';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { Outlet, useParams } from 'react-router-dom';

import * as styled from './styled';

interface IScheduleProps {}

export const Schedule: React.FC<IScheduleProps> = () => {
  const { values } = useFormikContext<IDataSourceModel>();
  const { id } = useParams();

  return values.schedules.length ? (
    <styled.Schedule className="schedule">
      <p>
        A service schedule provides a way to manage when and how often source content is imported.
      </p>
      <Tabs
        tabs={
          <>
            <Tab
              exact
              navigateTo="continuous"
              label="Continuous"
              activePaths={[`/admin/data/sources/${id}/schedules`]}
            />
            <Tab exact navigateTo="daily" label="Start/Stop" />
            <Tab exact navigateTo="advanced" label="Advanced" />
          </>
        }
      >
        <Outlet />
      </Tabs>
    </styled.Schedule>
  ) : null;
};
