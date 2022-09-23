import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import { TimeZones } from './constants';
import * as styled from './styled';

export const AudioTuner: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

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
    </styled.IngestType>
  );
};
