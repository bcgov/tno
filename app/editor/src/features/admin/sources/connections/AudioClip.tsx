import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import { TimeZones } from './constants';
import * as styled from './styled';

export const AudioClip: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  return (
    <styled.MediaType>
      <FormikSelect label="Timezone" name="connection.timeZone" options={TimeZones} />
      <FormikText
        label="Clip Command"
        name="connection.clipCmd"
        value={values.connection.clipCmd}
      />
      <FormikText
        label="Volume Range"
        name="connection.volumeRange"
        value={values.connection.volumeRange}
      />
    </styled.MediaType>
  );
};
