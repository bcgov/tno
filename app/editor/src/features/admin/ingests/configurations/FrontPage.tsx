import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import * as styled from './styled';

export const FrontPage: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  useTooltips();

  return (
    <styled.IngestType>
      <FormikText
        label="Path to Files"
        name="configuration.path"
        value={values.configuration.path}
      />
      <FormikText
        label="File Name Expression (i.e. Code)"
        name="configuration.fileName"
        value={values.configuration.fileName}
      />
    </styled.IngestType>
  );
};
