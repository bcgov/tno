import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import { TimeZones } from './constants';
import * as styled from './styled';

export const AudioTuner: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  return (
    <styled.MediaType>
      <FormikSelect label="Timezone" name="connection.timeZone" options={TimeZones} required />
      <FormikText
        label="Volume Range"
        name="connection.volumeRange"
        value={values.connection.volumeRange}
      />
      <FormikText
        label="Frequency"
        name="connection.frequency"
        value={values.connection.frequency}
      />
    </styled.MediaType>
  );
};
