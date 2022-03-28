import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import * as styled from './styled';

export const Syndication: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  return (
    <styled.MediaType>
      <h3>Connection Settings</h3>
      <FormikText label="URL" name="connection.url" value={values.connection.url} />
      <FormikText label="username" name="connection.username" value={values.connection.username} />
      <FormikText
        label="password"
        name="connection.password"
        value={values.connection.password}
        type="password"
        autoComplete="off"
      />
    </styled.MediaType>
  );
};
