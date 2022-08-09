import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { Show } from 'tno-core';
import { getSortableOptions } from 'utils';

import * as styled from './styled';

export const Image: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();
  const [lookups] = useLookup();

  const dataLocations = getSortableOptions(lookups.dataLocations);
  const dataLocation = dataLocations.find((t) => t.value === values.connection.dataLocationId);

  return (
    <styled.MediaType>
      <FormikSelect
        label="Data Location"
        name="connection.dataLocationId"
        tooltip="The physical location that files are stored before ingesting"
        options={dataLocations}
        value={dataLocation}
        required
      />
      <Show visible={['SFTP', 'FTP'].includes(dataLocation?.label ?? '')}>
        <FormikText
          label="Username"
          name="connection.username"
          value={values.connection.username}
        />
        <FormikText
          label="Password"
          name="connection.password"
          value={values.connection.password}
          type="password"
        />
        <FormikText
          label="Filename"
          name="connection.filename"
          value={values.connection.filename}
        />
      </Show>
    </styled.MediaType>
  );
};
