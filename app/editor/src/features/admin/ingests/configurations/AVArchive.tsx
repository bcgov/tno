import { useFormikContext } from 'formik';
import React from 'react';
import { FormikText, IIngestModel } from 'tno-core';

import * as styled from './styled';

export const AVArchive: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  return (
    <styled.IngestType>
      <FormikText
        label="Volume Range"
        name="configuration.volumeRange"
        value={values.configuration.volumeRange}
      />
      <FormikText
        label="Frame Rate"
        name="configuration.frameRate"
        value={values.configuration.frameRate}
      />
    </styled.IngestType>
  );
};
