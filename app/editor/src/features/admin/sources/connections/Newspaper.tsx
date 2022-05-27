import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import * as styled from './styled';

export const Newspaper: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  return (
    <styled.MediaType>
      <FormikText
        label="Page Numbers"
        name="connection.pageNumbers"
        value={values.connection.pageNumbers}
      />
      <FormikText label="Sections" name="connection.sections" value={values.connection.sections} />
    </styled.MediaType>
  );
};
