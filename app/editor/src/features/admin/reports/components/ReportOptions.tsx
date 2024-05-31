import { Col, FormikCheckbox, Row } from 'tno-core';

export const ReportOptions = () => {
  return (
    <Col className="frm-in options" flex="1">
      <div>
        <label>Report Options</label>
      </div>
      <Row gap="1rem" nowrap>
        <Col className="frm-in" flex="1">
          <label>Format options</label>
          <FormikCheckbox
            label="Show Link to Story"
            name="settings.content.showLinkToStory"
            tooltip="Include a link to view the story online"
          />
          <FormikCheckbox
            label="Highlight Keywords"
            name="settings.content.highlightKeywords"
            tooltip="Highlight the search keywords found in the report content"
          />
          <FormikCheckbox
            label="Use Page Breaks"
            name="settings.sections.usePageBreaks"
            tooltip="use page breaks in each section for printing."
          />
          <p>
            Control the output of the report. Redirect users to view the report on the website
            instead of in their email.
          </p>
        </Col>
        <Col className="frm-in" flex="1">
          <label>Generation options</label>
          <FormikCheckbox
            name={`settings.content.copyPriorInstance`}
            label="Copy prior report when starting new report"
          />
          <FormikCheckbox
            name={`settings.content.clearOnStartNewReport`}
            label="Empty report when starting next report"
            tooltip="If this is not set an auto report will not generate a new report if an active one already exists."
          />
          <FormikCheckbox
            name={`settings.content.excludeContentInUnsentReport`}
            label="Exclude content found in unsent report"
            tooltip="Excluding content in the current unsent report ensures each time the report is generated it will only have new content."
          />
        </Col>
      </Row>
    </Col>
  );
};
