import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import { Languages, TimeZones } from './constants';
import * as styled from './styled';

export const VideoTuner: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);
  const language = Languages.find((t) => t.value === values.configuration.language);

  return (
    <styled.IngestType>
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        options={TimeZones}
        value={timeZone}
        required
      />
      <FormikSelect
        label="Language"
        name="configuration.language"
        options={Languages}
        value={language}
      />
      <FormikText
        label="Volume"
        name="configuration.volume"
        tooltip="Volume in percent or dB (1 = 100%)"
        placeholder="1"
      />
      <FormikText
        label="Frequency"
        name="configuration.frequency"
        value={values.configuration.frequency}
      />
    </styled.IngestType>
  );
};
