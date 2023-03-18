import { useFormikContext } from 'formik';
import React from 'react';
import { Col, FormikSelect, FormikText, IIngestModel, Row } from 'tno-core';

import { Languages, LoggingLevels, TimeZones } from './constants';
import * as styled from './styled';

export const AudioTuner: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  const language = Languages.find((t) => t.value === values.configuration.language);
  const logLevel = LoggingLevels.find((t) => t.value === values.configuration.logLevel);

  return (
    <styled.IngestType>
      <FormikSelect label="Timezone" name="configuration.timeZone" options={TimeZones} required />
      <FormikText
        label="Volume Range"
        name="configuration.volumeRange"
        value={values.configuration.volumeRange}
      />
      <FormikText
        label="Frequency"
        name="configuration.frequency"
        value={values.configuration.frequency}
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
    </styled.IngestType>
  );
};
