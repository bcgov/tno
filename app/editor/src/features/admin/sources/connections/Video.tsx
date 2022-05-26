import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import * as styled from './styled';

export const Video: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  return (
    <styled.MediaType>
      <FormikText
        label="Volume Range"
        name="connection.volumeRange"
        value={values.connection.volumeRange}
      />
      <FormikText
        label="Frame Rate"
        name="connection.frameRate"
        value={values.connection.frameRate}
      />
    </styled.MediaType>
  );
};
