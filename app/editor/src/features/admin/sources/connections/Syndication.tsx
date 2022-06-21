import { FieldSize } from 'components/form';
import { FormikCheckbox, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import * as styled from './styled';

export const Syndication: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();

  return (
    <styled.MediaType>
      <FormikText
        label="URL"
        name="connection.url"
        value={values.connection.url}
        width={FieldSize.Medium}
      />
      <FormikText label="Username" name="connection.username" value={values.connection.username} />
      <FormikText
        label="Password"
        name="connection.password"
        value={values.connection.password}
        type="password"
        autoComplete="off"
      />
      <p>Do not turn on "Import Content" until you have successfully ingested content.</p>
      <FormikCheckbox
        label="Import Content"
        name="connection.import"
        tooltip="Whether ingested content should be imported"
        onChange={(e) => {
          setFieldValue('connection.import', e.currentTarget.checked);
        }}
      />
    </styled.MediaType>
  );
};
