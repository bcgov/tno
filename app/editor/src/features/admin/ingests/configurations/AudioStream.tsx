import { useFormikContext } from 'formik';
import React from 'react';
import { Col, FormikSelect, FormikText, IIngestModel, Row, useFormikHelpers } from 'tno-core';

import { Languages, LoggingLevels, TimeZones } from './constants';
import { ImportContent } from './ImportContent';
import * as styled from './styled';

export const AudioStream: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  const { applyPlaceholder } = useFormikHelpers();

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);
  const language = Languages.find((t) => t.value === values.configuration.language);
  const logLevel = LoggingLevels.find((t) => t.value === values.configuration.logLevel);

  return (
    <styled.IngestType>
      <ImportContent />
      <FormikText
        label="Stream URL"
        name="configuration.url"
        tooltip="URL to the source stream"
        required
      />
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        value={timeZone}
      />
      <Row>
        <Col flex="1 1 0">
          <FormikSelect
            label="Language"
            name="configuration.language"
            options={Languages}
            value={language}
          />
        </Col>
        <Col flex="1 1 0">
          <FormikSelect
            label="Logging Level"
            name="configuration.logLevel"
            options={LoggingLevels}
            value={logLevel}
          />
        </Col>
      </Row>
      <FormikText label="Format" name="configuration.format" tooltip="Format of the stream" />
      <p>Use "{'{schedule.Name}'}.mp3" to name the file with the schedule name.</p>
      <FormikText
        label="Output File Name"
        name="configuration.fileName"
        tooltip="File name and output format"
        placeholder="{schedule.Name}.mp3"
        onClick={applyPlaceholder}
      />
      <FormikText
        label="Volume"
        name="configuration.volume"
        tooltip="Volume in percent or dB (1 = 100%)"
        placeholder="1"
        onClick={applyPlaceholder}
      />
      <FormikText
        label="Other Arguments"
        name="configuration.otherArgs"
        tooltip="Any other arguments to pass to the command"
        placeholder="-acodec mp3 -ab 257k"
        onClick={applyPlaceholder}
      />
    </styled.IngestType>
  );
};
