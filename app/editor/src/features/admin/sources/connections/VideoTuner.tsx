import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import { TimeZones } from './constants';
import * as styled from './styled';

export const VideoTuner: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  return (
    <styled.MediaType>
      <FormikSelect label="Timezone" name="connection.timeZone" options={TimeZones} required />
      <FormikText
        label="Volume"
        name="connection.volume"
        tooltip="Volume in percent or dB (1 = 100%)"
        placeholder="1"
      />
      <FormikText
        label="Frequency"
        name="connection.frequency"
        value={values.connection.frequency}
      />
    </styled.MediaType>
  );
};
