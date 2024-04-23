import { Button } from 'components/button';
import { hideEmptySections } from 'features/my-reports/utils';
import React from 'react';
import { FaInfoCircle, FaSyncAlt } from 'react-icons/fa';
import { useReports } from 'store/hooks';
import {
  Checkbox,
  FormikCheckbox,
  FormikSelect,
  getSortableOptions,
  IOptionItem,
  Row,
} from 'tno-core';

import { useReportEditContext } from '../ReportEditContext';
import * as styled from './styled';

export const ReportEditPreferencesForm = () => {
  const { setFieldValue, values, setValues } = useReportEditContext();
  const [, { findMyReports }] = useReports();

  const [reportOptions, setReportOptions] = React.useState<IOptionItem[]>([]);
  const [excludeContentInReports, setExcludeContentInReports] = React.useState(
    !!values.settings.content.excludeReports.length,
  );

  const fetchMyReports = async () => {
    try {
      const reports = await findMyReports();
      setReportOptions(getSortableOptions(reports));
    } catch (error) {
      throw error;
    }
  };

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
          disabled
        />
        <Row>
          <FormikCheckbox
            name="settings.content.onlyNewContent"
            label="Include only new content posted after previous report"
          />
          <span className="info">
            Only include content that has been 'posted' after the report has been 'created'. Posted
            content is any content that an MMI Editor publishes.
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
    </styled.ReportEditPreferencesForm>
  );
};
