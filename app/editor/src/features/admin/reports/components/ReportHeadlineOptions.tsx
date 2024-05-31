import { Col, FormikCheckbox } from 'tno-core';

export const ReportHeadlineOptions = () => {
  return (
    <Col className="frm-in options" flex="1">
      <div>
        <label>Headline Options</label>
      </div>
      <Col>
        <p>Control what is displayed for each headline in the report.</p>
        <FormikCheckbox label="Show Source" name="settings.headline.showSource" />
        <FormikCheckbox label="Show Common Call" name="settings.headline.showShortName" />
        <FormikCheckbox label="Show Published On" name="settings.headline.showPublishedOn" />
        <FormikCheckbox label="Show Byline" name="settings.headline.showByline" />
        <FormikCheckbox label="Show Sentiment" name="settings.headline.showSentiment" />
      </Col>
    </Col>
  );
};
