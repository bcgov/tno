import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import * as styled from './styled';

export const Newspaper: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  return (
    <styled.IngestType>
      <FormikText
        label="Page Numbers"
        name="configuration.pageNumbers"
        value={values.configuration.pageNumbers}
      />
      <FormikText
        label="Sections"
        name="configuration.sections"
        value={values.configuration.sections}
      />
    </styled.IngestType>
  );
};
