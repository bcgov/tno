import { useFormikContext } from 'formik';
import React from 'react';
import { useReports } from 'store/hooks/admin';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  getSortableOptions,
  IReportModel,
  OptionItem,
  Row,
} from 'tno-core';

export const ReportContentOptions = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ reports }, { findReports }] = useReports();
  const [reportOptions, setReportOptions] = React.useState(getSortableOptions(reports));

  React.useEffect(() => {
    if (!reports.length)
      findReports({})
        .then((reports) => {
          setReportOptions(getSortableOptions(reports));
        })
        .catch(() => {});
    // Only run on initialize.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <Col className="frm-in options" flex="2">
      <div>
        <label>Content Options</label>
      </div>
      <Col>
        <p>
          Control what content is included in this report by removing content from prior instances
          or related reports.
        </p>
        <Row gap="1rem">
          <Col gap="1rem">
            <FormikCheckbox
              label="Exclude stories that have been sent out in previous report"
              name="settings.content.excludeHistorical"
              tooltip="Exclude content already reported on in prior instances of this report"
            />
            <FormikCheckbox
              label="Remove duplicate titles from the last 3 days"
              name="settings.content.removeDuplicateTitles3Days"
              tooltip="Keep only the most recent story when duplicate titles are published within the past three days."
            />
            <FormikSelect
              label="Excludes content already reported on in the selected reports"
              name="settings.content.excludeReports"
              tooltip="Excludes content already reported on in the selected reports"
              options={reportOptions}
              isMulti
              value={reportOptions.filter((ro) =>
                values.settings.content.excludeReports?.some((reportId) => reportId === ro.value),
              )}
              onChange={(newValue) => {
                if (Array.isArray(newValue))
                  setFieldValue(
                    'settings.content.excludeReports',
                    newValue.map((v: OptionItem) => v.value),
                  );
              }}
            />
          </Col>
          <Col gap="1rem">
            <FormikCheckbox
              label="Include only new content posted after previous report"
              name="settings.content.onlyNewContent"
              tooltip="Adds a date filter to only include content posted since the last time this report ran"
            />
            <FormikCheckbox
              label="Clear all folders after report runs"
              name="settings.content.clearFolders"
              tooltip="Clears all content from all folders in this report after this report is run"
            />
            <FormikCheckbox
              label="Omit 'BC Updates' and 'BC Calendar' from the report"
              name="settings.content.omitBCUpdates"
              tooltip="Omits 'BC Updates' and 'BC Calendar' from the report"
            />
          </Col>
        </Row>
      </Col>
    </Col>
  );
};
