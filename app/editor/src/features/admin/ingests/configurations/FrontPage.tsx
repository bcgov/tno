import { useFormikContext } from 'formik';
import React from 'react';
import { Col, FormikCheckbox, FormikSelect, FormikText, IIngestModel, Row } from 'tno-core';

import { TimeZones } from './constants';
import { ImportContent } from './ImportContent';
import * as styled from './styled';

export const FrontPage: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  return (
    <styled.IngestType>
      <ImportContent />
      <FormikText
        label="Path to Files"
        name="configuration.path"
        value={values.configuration.path}
        tooltip="Path to files can include '<date>' which will be replaced with today's date"
      />
      <FormikText
        label="Path Date Format"
        name="configuration.pathDateFormat"
        value={values.configuration.pathDateFormat}
        tooltip="Format the date and replace the <date> keyword in the path (i.e. yyyy/MM/dd)"
      />
      <FormikText
        label="File Name Expression"
        name="configuration.fileName"
        value={values.configuration.fileName}
        tooltip="Regular expression can include '<date>' which will be replaced with today's date"
        placeholder="name-<date>\.jpg"
      />
      <Row>
        <Col flex="1">
          <FormikText
            label="File Name Date Format"
            name="configuration.dateFormat"
            value={values.configuration.dateFormat}
            tooltip="Format the date and replace the <date> keyword in the file name expression (i.e. ddd-ddMMyyyy)"
          />
        </Col>
        <Col justifyContent="center">
          <FormikCheckbox
            label="Uppercase Date"
            name="configuration.uppercaseDate"
            value={values.configuration.uppercaseDate}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikSelect
            label="Timezone"
            name="configuration.timeZone"
            tooltip="Timezone of the source"
            options={TimeZones}
            value={values.configuration.timeZone}
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="Date Offset"
            name="configuration.dateOffset"
            type="number"
            tooltip="Which day to parse (0 = today)"
          />
        </Col>
      </Row>
      <p>
        Front page images are often delivered the evening before the publish date. Use a regular
        expression to parse the file name to determine the published on date. Use regex group names
        to identify each part of the date, (?&lt;year&gt;) (?&lt;month&gt;) (?&lt;day&gt;)
        (?&lt;hour&gt;) (?&lt;minute&gt;) (?&lt;second&gt;)
      </p>
      <FormikText
        label="Parse Published On from File Name"
        name="configuration.publishedOnExpression"
        tooltip="Use a regex to extract the published on date"
        placeholder="^sv-GLB-[A-Za-z]{3}-(?<day>[0-9]{2})(?<month>[0-9]{2})(?<year>[0-9]{4})"
        value={values.configuration.publishedOnExpression}
      />
      <FormikCheckbox label="Publish" name="configuration.publish" />
    </styled.IngestType>
  );
};
