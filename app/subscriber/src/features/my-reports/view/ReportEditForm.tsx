import { Action } from 'components/action';
import { Tabs } from 'components/tabs';
import { ITab } from 'components/tabs/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { useParams } from 'react-router-dom';

import { IReportForm } from '../interfaces';
import { ReportContentForm, ReportPreviewForm, ReportSendForm } from './components';

export interface IReportEditFormProps {
  disabled?: boolean;
}

export const ReportEditForm: React.FC<IReportEditFormProps> = ({ disabled }) => {
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
        label: <Action label={`${disabled ? 'View' : 'Edit'} Content`} />,
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
    [disabled, values.id, values.name],
  );

  return (
    <Tabs tabs={tabs} activeTab={path}>
      {(tab) => {
        if (tab?.key === 'preview') return <ReportPreviewForm />;
        else if (tab?.key === 'send') return <ReportSendForm />;
        return <ReportContentForm disabled={disabled} />;
      }}
    </Tabs>
  );
};
