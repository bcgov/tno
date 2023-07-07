import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Show, Tab, Tabs } from 'tno-core';

import { ChartList } from '../charts';
import { ReportList, ReportTemplateList } from '.';
import * as styled from './styled';

export interface IReportAdminProps {
  path?: string;
}

/**
 * Provides a page to admin a list of reports and report templates.
 * @returns Component to admin reports and report templates.
 */
export const ReportAdmin: React.FC<IReportAdminProps> = ({ path: defaultPath = 'reports' }) => {
  const navigate = useNavigate();
  const { path = defaultPath } = useParams();

  return (
    <styled.ReportAdmin>
      <Tabs
        tabs={
          <>
            <Tab
              label="Reports"
              onClick={() => {
                navigate('/admin/reports');
              }}
              active={path === 'reports'}
            />
            <Tab
              label="Report Templates"
              onClick={() => {
                navigate('/admin/report/templates');
              }}
              active={path === 'templates'}
            />
            <Tab
              label="Chart Templates"
              onClick={() => {
                navigate('/admin/chart/templates');
              }}
              active={path === 'charts'}
            />
          </>
        }
      >
        <Show visible={path === 'reports'}>
          <ReportList />
        </Show>
        <Show visible={path === 'templates'}>
          <ReportTemplateList />
        </Show>
        <Show visible={path === 'charts'}>
          <ChartList />
        </Show>
      </Tabs>
    </styled.ReportAdmin>
  );
};
