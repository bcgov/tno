import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import * as styled from './styled';

export const Audio: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  return (
    <styled.MediaType>
      <h3>Connection Settings</h3>
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
      <FormikText
        label="Stream URL"
        name="connection.audioUrl"
        value={values.connection.audioUrl}
      />
      <FormikText
        label="Capture Directory"
        name="connection.captureDir"
        value={values.connection.captureDir}
      />
      <FormikText
        label="Clip Directory"
        name="connection.clipDir"
        value={values.connection.clipDir}
      />
      <FormikText
        label="Capture Command"
        name="connection.captureCmd"
        value={values.connection.captureCmd}
      />
      <FormikText
        label="Stream Timeout"
        name="connection.streamTimeout"
        value={values.connection.streamTimeout}
      />
      <FormikText
        label="Clip Command"
        name="connection.clipCmd"
        value={values.connection.clipCmd}
      />
      <FormikText label="Timezone" name="connection.timeZone" value={values.connection.timeZone} />
    </styled.MediaType>
  );
};
