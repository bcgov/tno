import { Action } from 'components/action';
import { Tabs } from 'components/tabs';
import { ITab } from 'components/tabs/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { useParams } from 'react-router-dom';
import { Col } from 'tno-core';

import { IReportForm } from '../interfaces';
import { ReportContentForm, ReportPreviewForm, ReportSendForm } from './components';

export const ReportEditForm: React.FC = () => {
  const { values } = useFormikContext<IReportForm>();
  const { path = 'content' } = useParams();

  const tabs: ITab[] = React.useMemo(
    () => [
      {
        key: 'id',
        type: 'other',
        label: values.name ? values.name : '[Report Name]',
        className: 'report-name',
      },
      {
        key: 'content',
        to: `/reports/${values.id}/edit/content`,
        label: <Action label="Edit Content" />,
      },
      {
        key: 'preview',
        to: `/reports/${values.id}/edit/preview`,
        label: <Action label="Preview" />,
      },
      {
        key: 'send',
        to: `/reports/${values.id}/edit/send`,
        label: <Action label="Send" />,
      },
    ],
    [values.id, values.name],
  );

  return (
    <div>
      <Col flex="1">
        <Tabs tabs={tabs} activeTab={path}>
          {(tab) => {
            if (tab?.key === 'preview') return <ReportPreviewForm />;
            else if (tab?.key === 'send') return <ReportSendForm />;
            return <ReportContentForm />;
          }}
        </Tabs>
      </Col>
    </div>
  );
};
