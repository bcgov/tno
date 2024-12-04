import { Button } from 'components/button';
import { hideEmptySections } from 'features/my-reports/utils';
import React from 'react';
import { FaInfoCircle, FaSyncAlt } from 'react-icons/fa';
import { useReports } from 'store/hooks';
import {
  Checkbox,
  Col,
  FormikCheckbox,
  FormikSelect,
  getReportKind,
  getSortableOptions,
  IOptionItem,
  ReportKindName,
  Row,
  Show,
} from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';

export const ReportEditPreferencesForm = () => {
  const { setFieldValue, values, setValues } = useReportEditContext();
  const [{ myReports }, { findMyReports }] = useReports();

  const [reportOptions, setReportOptions] = React.useState<IOptionItem[]>([]);
  const [excludeContentInReports, setExcludeContentInReports] = React.useState(
    !!values.settings.content.excludeReports.length,
  );

  const kind = getReportKind(values);

  const fetchMyReports = async () => {
    try {
      const reports = await findMyReports();
      setReportOptions(getSortableOptions(reports));
    } catch (error) {
      throw error;
    }
  };

  React.useEffect(() => {
    setReportOptions(getSortableOptions(myReports));
  }, [myReports]);

  return (
    <styled.ReportEditPreferencesForm className="report-edit-section">
      <div className="info">
        <div>
          <FaInfoCircle />
          <label>More info...</label>
        </div>
        The preferences set here will affect how the content displays throughout this report.
      </div>

      <div className="frm-in">
        <label>Select the fields to show on report:</label>
        <FormikCheckbox name="settings.headline.showSource" label="Source" />
        <FormikCheckbox name="settings.headline.showShortName" label="Common call" />
        <FormikCheckbox name="settings.headline.showPublishedOn" label="Published on" />
        <FormikCheckbox name="settings.headline.showByline" label="Byline (author)" />
        <FormikCheckbox name="settings.headline.showSentiment" label="Sentiment rating" />
      </div>

      <div className="frm-in">
        <label>Content format:</label>
        <FormikCheckbox name="settings.content.showLinkToStory" label="Include links to stories" />
        <FormikCheckbox
          name="settings.sections.usePageBreaks"
          label="Use page breaks when formatting reports"
        />
        <FormikCheckbox
          name="settings.content.highlightKeywords"
          label="Highlight search terms in body text"
        />
        <FormikCheckbox
          name="hideEmptySections"
          label="Hide sections that contain no stories"
          onChange={(e) => {
            setValues(hideEmptySections(values, e.target.checked));
          }}
        />
      </div>

      <div className="frm-in">
        <label>Content filtering:</label>
        <FormikCheckbox
          name="settings.content.omitBCUpdates"
          label="Omit all BC Updates and BC Calendars from report"
        />
        <Row>
          <FormikCheckbox
            name="settings.content.onlyNewContent"
            label="Include only new content posted after previous report"
          />
          <span className="info">
            Only include content that has been 'posted' after the report has been 'created'. The
            posted date is when an Editor saves the content in MMI.
          </span>
        </Row>
        <FormikCheckbox
          name="settings.content.excludeHistorical"
          label="Exclude stories that have been sent out in previous report"
        />
        <Checkbox
          label="Exclude stories that appeared in the following report(s):"
          checked={excludeContentInReports}
          onChange={(e) => {
            if (!e.target.checked) {
              setFieldValue('settings.content.excludeReports', []);
            }
            setExcludeContentInReports(e.target.checked);
          }}
        />
        <div className="select">
          <FormikSelect
            name="settings.content.excludeReports"
            tooltip="Excludes content already reported on in the selected reports"
            options={reportOptions}
            isMulti
            isDisabled={!excludeContentInReports}
            value={reportOptions.filter((ro) =>
              values.settings.content.excludeReports?.some((reportId) => reportId === ro.value),
            )}
            onChange={(newValue) => {
              if (Array.isArray(newValue))
                setFieldValue(
                  'settings.content.excludeReports',
                  newValue.map((v: IOptionItem) => v.value),
                );
            }}
          >
            <Button onClick={() => fetchMyReports()}>
              <FaSyncAlt />
            </Button>
          </FormikSelect>
        </div>
        <FormikCheckbox
          name="settings.content.clearFolders"
          label="Clear all folders included in this report after this report runs"
        />
      </div>
      <div className="frm-in">
        <label>Report Generation Options:</label>
        <Row gap="1rem">
          <Show visible={kind !== ReportKindName.Manual}>
            <Col>
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
            <Col>
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
                  When you generate the next report it will empty out any content in the current
                  unsent report.
                </p>
              </Show>
            </Col>
          </Show>
        </Row>
      </div>
      <div className="frm-in">
        <label>Email sending options:</label>
        <Row gap="1rem">
          <Col>
            <Checkbox
              name={`settings.doNotSendEmail`}
              label="Do not send Email"
              checked={values.settings.doNotSendEmail}
              onChange={(e) => {
                setFieldValue('settings.doNotSendEmail', e.target.checked);
              }}
            />
          </Col>
          <p className="info">
            Use this option when you want a report to run and act like it was sent out, but not
            actually send the email. Useful when testing a new report.
          </p>
        </Row>
      </div>
    </styled.ReportEditPreferencesForm>
  );
};
