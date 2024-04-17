import { Button } from 'components/button';
import { Modal } from 'components/modal';
import { IReportForm } from 'features/my-reports/interfaces';
import React from 'react';
import { FaInfoCircle } from 'react-icons/fa';
import { FaPaperPlane } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useReportInstances } from 'store/hooks';
import {
  formatDate,
  FormikCheckbox,
  FormikText,
  ReportStatusName,
  Show,
  Spinner,
  useModal,
} from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';
import { ReportSchedule } from './template';

export interface IReportEditSendFormProps {
  /** Event to update the original report. */
  updateForm: (values: IReportForm) => void;
}

export const ReportEditSendForm = ({ updateForm }: IReportEditSendFormProps) => {
  const { onGenerate, values, setFieldValue } = useReportEditContext();
  const [{ publishReportInstance }] = useReportInstances();
  const { toggle: toggleStartNewReport, isShowing: isShowingStartNewReport } = useModal();
  const { toggle: toggleSend, isShowing: isShowingSend } = useModal();

  const instance = values.instances.length ? values.instances[0] : undefined;

  const handlePublish = React.useCallback(
    async (id: number) => {
      try {
        const updatedInstance = await publishReportInstance(id);
        setFieldValue(
          'instances',
          values.instances.map((i) =>
            i.id === id ? { ...updatedInstance, content: instance?.content } : i,
          ),
        );
        toast.success(
          'Report has been submitted.  You will be notified when it is emailed to subscribers.',
        );
      } catch {}
    },
    [instance?.content, publishReportInstance, setFieldValue, values.instances],
  );

  const handleStartNewReport = React.useCallback(
    async (values: IReportForm) => {
      try {
        const form = await onGenerate(values, true);
        if (form) updateForm(form);
      } catch {}
    },
    [onGenerate, updateForm],
  );

  return (
    <styled.ReportEditSendForm>
      <h2>Email options</h2>
      <div>
        <FormikText name="settings.subject.text" label="Email subject line:" required />
        <FormikCheckbox
          name="settings.subject.showTodaysDate"
          label="Append the report date to the subject line"
        />
      </div>

      <h2>Scheduling</h2>
      <div className="info">
        <div>
          <FaInfoCircle /> Scheduling...
        </div>
        <p>
          Scheduling a report will automatically populate it at the specified day and time. If the
          'Auto send' option is selected it will also send to its subscribers automatically. Each
          report can have up to TWO schedules. The second schedule may be used for sending out
          reports at a different time on certain days.
        </p>
        <p>
          A schedule that has already populated a report in a given day, will not attempt to do so
          again unless you allow it to 'Run again today'.
        </p>
      </div>

      <div className="schedules">
        <ReportSchedule index={0} label="Schedule 1" />
        <ReportSchedule index={1} label="Schedule 2" />
        <div className="schedule-actions">
          <Show visible={!!instance && instance.status === ReportStatusName.Submitted}>
            <p>Report has been published and is being sent to subscribers</p>
            <p>Status: {instance?.status}</p>
            <Spinner />
          </Show>
          <Show visible={!instance || !!instance?.sentOn}>
            <Button variant="success" onClick={() => toggleStartNewReport()}>
              Start next report
            </Button>
          </Show>
          <Show visible={!instance?.sentOn && instance?.status === ReportStatusName.Pending}>
            <Button variant="success" onClick={() => instance && toggleSend()}>
              Send current report to subscribers now
              <FaPaperPlane />
            </Button>
          </Show>
        </div>
      </div>
      <Modal
        headerText="Send Report to Subscribers"
        body={`Do you want to send an email to the subscribers of this report? ${
          instance?.sentOn ? 'This report has already been sent out by email.' : ''
        }`}
        isShowing={isShowingSend}
        hide={toggleSend}
        type="default"
        confirmText="Yes, send report to subscribers"
        onConfirm={async () => {
          try {
            if (instance) await handlePublish(instance.id);
          } finally {
            toggleSend();
          }
        }}
      />
      <Modal
        headerText="Start Next Report"
        body={
          <>
            <p>{`The current report was sent to subscribers on ${formatDate(
              instance?.sentOn?.toLocaleString(),
              'YYYY-MM-DD hh:mm:ss a',
            )}.`}</p>
            <p>Would you like to start the next report?</p>
          </>
        }
        isShowing={isShowingStartNewReport}
        hide={toggleStartNewReport}
        type="default"
        confirmText="Yes, start the next report"
        onConfirm={async () => {
          try {
            await handleStartNewReport(values);
          } finally {
            toggleStartNewReport();
          }
        }}
      />
    </styled.ReportEditSendForm>
  );
};
