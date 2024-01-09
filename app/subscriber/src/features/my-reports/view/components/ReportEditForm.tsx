import { Action } from 'components/action';
import { Tabs } from 'components/tabs';
import { ITab } from 'components/tabs/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaRecycle } from 'react-icons/fa';
import { useParams } from 'react-router-dom';
import { useProfileStore } from 'store/slices';
import { Row } from 'tno-core';

import { IReportForm } from '../../interfaces';
import { ReportPreviewForm, ReportSections, ReportSendForm } from '.';

export interface IReportEditFormProps {
  disabled?: boolean;
}

export const ReportEditForm: React.FC<IReportEditFormProps> = ({ disabled }) => {
  const { values } = useFormikContext<IReportForm>();
  const { path = 'content' } = useParams();
  const [, { storeReportOutput }] = useProfileStore();

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
        label: (
          <Row gap="1rem">
            <Action label="Preview" />
            <Action icon={<FaRecycle />} onClick={() => storeReportOutput(undefined)} />
          </Row>
        ),
      },
      {
        key: 'send',
        to: `/reports/${values.id}/edit/send`,
        label: <Action label="Send" />,
      },
    ],
    [disabled, storeReportOutput, values.id, values.name],
  );

  return (
    <Tabs tabs={tabs} activeTab={path}>
      {(tab) => {
        if (tab?.key === 'preview') return <ReportPreviewForm />;
        else if (tab?.key === 'send') return <ReportSendForm />;
        return <ReportSections disabled={disabled} />;
      }}
    </Tabs>
  );
};
