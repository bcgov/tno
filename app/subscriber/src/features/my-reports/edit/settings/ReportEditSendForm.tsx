import { Button } from 'components/button';
import { IToggleOption, Toggle } from 'components/form';
import { FaInfoCircle } from 'react-icons/fa';
import { Checkbox, Col, FormikCheckbox, FormikText, ReportKindName, Row, Show } from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';
import { ReportSchedule } from './template';

export interface IReportEditSendFormProps {
  /** event to publish the report and send to subscribers. */
  onPublish: () => void;
  /** Event to request starting next report. */
  onGenerate: () => void;
}

export const ReportEditSendForm = ({ onPublish, onGenerate }: IReportEditSendFormProps) => {
  const { values, setValues, setFieldValue } = useReportEditContext();

  const instance = values.instances.length ? values.instances[0] : undefined;
  const kind = values.events.some((e) => e.isEnabled) ? ReportKindName.Auto : ReportKindName.Manual;
  const reportOptions: IToggleOption<ReportKindName>[] = [
    { label: 'Manual Report', value: ReportKindName.Manual },
    { label: 'Auto Report', value: ReportKindName.Auto },
    { label: 'Auto Send Report', value: ReportKindName.AutoSend },
  ];
  const isAuto = [ReportKindName.Auto, ReportKindName.AutoSend].includes(kind);

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
      <Col className="info" flex="1">
        <div>
          <FaInfoCircle /> Scheduling info...
        </div>
        <p>
          Scheduling an 'Auto Report' will automatically populate it at the specified day and time.
          An 'Auto Send Report' will also send to its subscribers automatically. Each report can
          have up to two schedules. The second schedule may be used for sending out reports at a
          different time on certain days.
        </p>
      </Col>
      <div>
        <Row nowrap gap="1rem" className="report-kind">
          <Col className="info" flex="1">
            <p>A Manual Report is created and sent manually.</p>
          </Col>
          <Col className="info" flex="1">
            <p>An Auto Report is created for you on a schedule, but sent manually.</p>
          </Col>
          <Col className="info" flex="1">
            <p>An Auto Send Report is created for you on a schedule and sent at the same time.</p>
          </Col>
        </Row>
      </div>
      <Toggle
        name="reportKind"
        options={reportOptions}
        value={kind}
        onChange={(value) => {
          const auto = [ReportKindName.Auto, ReportKindName.AutoSend].includes(value);
          const autoSend = value === ReportKindName.AutoSend;
          setValues({
            ...values,
            settings: {
              ...values.settings,
              content: { ...values.settings.content, copyPriorInstance: false },
            },
            events: values.events.map((e) => ({
              ...e,
              isEnabled: auto,
              settings: { ...e.settings, autoSend: autoSend },
            })),
          });
        }}
      />
      <Show visible={isAuto}>
        <div className="schedules">
          <ReportSchedule index={0} label="Schedule 1" />
          <ReportSchedule index={1} label="Schedule 2" />
        </div>
        <Col className="info" flex="1">
          <p>
            A schedule that has already populated a report in a given day, will not attempt to do so
            again unless you allow it to 'Run again today'.
          </p>
        </Col>
      </Show>
      <Show visible={kind === ReportKindName.Manual}>
        <div className="schedule-actions">
          <Col className="info" flex="1">
            <p>
              A schedule that has already populated a report in a given day, will not attempt to do
              so again unless you allow it to 'Run again today'.
            </p>
          </Col>
          <Checkbox
            name={`settings.content.copyPriorInstance`}
            label="Empty report when starting next report"
            checked={!values.settings.content.copyPriorInstance}
            onChange={(e) => {
              setFieldValue('settings.content.copyPriorInstance', !e.target.checked);
            }}
          />
          <Show visible={!instance || !!instance?.sentOn}>
            <p>The current report has already been sent to subscribers. Start the next report.</p>
            <Button variant="success" onClick={() => onGenerate()}>
              Start next report
            </Button>
          </Show>
        </div>
      </Show>
    </styled.ReportEditSendForm>
  );
};
