import { Button } from 'components/button';
import { IToggleOption, Toggle } from 'components/form';
import { FaInfoCircle } from 'react-icons/fa';
import { FaClock, FaUser, FaUserClock } from 'react-icons/fa6';
import {
  Checkbox,
  Col,
  FormikCheckbox,
  FormikText,
  getReportKind,
  ReportKindName,
  Row,
  Show,
} from 'tno-core';

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
  const kind = getReportKind(values);
  const reportOptions: IToggleOption<ReportKindName>[] = [
    {
      label: (
        <Row gap="1rem" alignItems="center">
          Manual Report
          <FaUser />
        </Row>
      ),
      value: ReportKindName.Manual,
    },
    {
      label: (
        <Row gap="1rem" alignItems="center">
          Auto Report
          <FaUserClock />
        </Row>
      ),
      value: ReportKindName.Auto,
    },
    {
      label: (
        <Row gap="1rem" alignItems="center">
          Auto Send Report
          <FaClock />
        </Row>
      ),
      value: ReportKindName.AutoSend,
    },
  ];
  const isAuto = [ReportKindName.Auto, ReportKindName.AutoSend].includes(kind);

  return (
    <styled.ReportEditSendForm className="report-edit-section">
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
        <p>
          <FaInfoCircle /> Scheduling an 'Auto Report' will automatically populate it at the
          specified day and time. An 'Auto Send Report' will also send to its subscribers
          automatically. Each report can have up to two schedules. The second schedule may be used
          for sending out reports at a different time on certain days.
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
              content: {
                ...values.settings.content,
                copyPriorInstance: auto ? false : values.settings.content.copyPriorInstance,
                excludeContentInUnsentReport: auto ? true : false,
                clearOnStartNewReport: auto,
              },
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
      </Show>

      <h2>Report generation options</h2>
      <Col className="info" flex="1">
        <p>
          <FaInfoCircle /> Control how each new report is generated. Build on the previous, or begin
          fresh with an empty report. Some options are exclusive and cannot be mixed.
        </p>
      </Col>
      <div className="schedule-actions">
        <Show visible={kind !== ReportKindName.Manual}>
          <Col flex="1" alignContent="center">
            <Checkbox
              name={`settings.content.clearOnStartNewReport`}
              label="Empty report when starting next report"
              checked={!!values.settings.content.clearOnStartNewReport}
              onChange={(e) => {
                setFieldValue('settings.content', {
                  ...values.settings.content,
                  clearOnStartNewReport: e.target.checked,
                  excludeContentInUnsentReport: e.target.checked,
                  copyPriorInstance: e.target.checked ? !e.target.checked : false,
                });
              }}
            />
            <Checkbox
              name={`settings.content.copyPriorInstance`}
              label="Accumulate content on each run until sent"
              checked={!!values.settings.content.copyPriorInstance}
              onChange={(e) => {
                setFieldValue('settings.content', {
                  ...values.settings.content,
                  clearOnStartNewReport: false,
                  excludeContentInUnsentReport: false,
                  copyPriorInstance: e.target.checked,
                });
              }}
            />
            <Checkbox
              name={`settings.content.excludeContentInUnsentReport`}
              label="Exclude content in current unsent report"
              checked={!!values.settings.content.excludeContentInUnsentReport}
              onChange={(e) => {
                setFieldValue('settings.content', {
                  ...values.settings.content,
                  excludeContentInUnsentReport: e.target.checked,
                });
              }}
            />
          </Col>
          <Col flex="1">
            <Show visible={values.settings.content.clearOnStartNewReport}>
              <p className="info">
                The next time this report auto runs it will start empty and then populate based on
                the section data sources.
              </p>
            </Show>
            <Show visible={values.settings.content.copyPriorInstance}>
              <p className="info">
                The next time this report auto runs it will accumulate new content based on the
                section data sources.
              </p>
            </Show>
            <Show visible={values.settings.content.excludeContentInUnsentReport}>
              <p className="info">
                Excluding content in the current unsent report ensures each time the report is
                generated it will only have new content.
              </p>
            </Show>
            <Show
              visible={
                !values.settings.content.clearOnStartNewReport &&
                !values.settings.content.copyPriorInstance &&
                !values.settings.content.excludeContentInUnsentReport
              }
            >
              <p className="info">The next time this report auto runs it will not change.</p>
            </Show>
          </Col>
        </Show>
        <Show visible={kind === ReportKindName.Manual}>
          <Col flex="1" alignContent="center">
            <Checkbox
              name={`settings.content.copyPriorInstance`}
              label="Empty report when starting next report"
              checked={!values.settings.content.copyPriorInstance}
              onChange={(e) => {
                setFieldValue('settings.content.copyPriorInstance', !e.target.checked);
              }}
            />
          </Col>
          <Col flex="1">
            <Show visible={values.settings.content.copyPriorInstance}>
              <p className="info">
                When you generate the next report it will copy the content from the prior report.
              </p>
            </Show>
            <Show visible={!values.settings.content.copyPriorInstance}>
              <p className="info">
                When you generate the next report it will empty out any content.
              </p>
            </Show>
          </Col>
        </Show>
      </div>
      <Show visible={kind === ReportKindName.Manual && (!instance || !!instance?.sentOn)}>
        <Col alignItems="center">
          <Row gap="1rem">
            <p>The current report has already been sent to subscribers. Start the next report.</p>
            <Button variant="success" onClick={() => onGenerate()}>
              Start next report
            </Button>
          </Row>
        </Col>
      </Show>
    </styled.ReportEditSendForm>
  );
};
