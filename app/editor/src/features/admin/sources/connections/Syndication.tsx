import { FormikCheckbox, FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { FieldSize } from 'tno-core';

import { TimeZones } from './constants';
import * as styled from './styled';

export const Syndication: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const timeZone = TimeZones.find((t) => t.value === values.connection.timeZone);

  return (
    <styled.MediaType>
      <FormikText
        label="URL"
        name="connection.url"
        value={values.connection.url}
        width={FieldSize.Large}
      />
      <FormikSelect
        label="Timezone"
        name="connection.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        defaultValue={timeZone}
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
      <FormikCheckbox
        label="Fetch Content Body Separately"
        name="connection.fetchContent"
        tooltip="Whether content body is located remotely"
        onChange={(e) => {
          setFieldValue('connection.fetchContent', e.currentTarget.checked);
        }}
      />
    </styled.MediaType>
  );
};
