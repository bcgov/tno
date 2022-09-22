import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import { TimeZones } from './constants';
import * as styled from './styled';

export const VideoTuner: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  return (
    <styled.MediaType>
      <FormikSelect label="Timezone" name="configuration.timeZone" options={TimeZones} required />
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
    </styled.MediaType>
  );
};
