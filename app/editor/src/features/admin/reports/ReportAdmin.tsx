import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Show, Tab, Tabs } from 'tno-core';

import { ReportList, ReportTemplateList } from '.';
import * as styled from './styled';

/**
 * Provides a page to admin a list of reports and report templates.
 * @returns Component to admin reports and report templates.
 */
export const ReportAdmin: React.FC = () => {
  const navigate = useNavigate();
  const { path = 'reports' } = useParams();

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
              label="Templates"
              onClick={() => {
                navigate('/admin/report/templates');
              }}
              active={path === 'templates'}
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
      </Tabs>
    </styled.ReportAdmin>
  );
};
