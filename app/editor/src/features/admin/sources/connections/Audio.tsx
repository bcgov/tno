import { FormikSelect } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import { AudioClip, AudioStream, AudioTuner } from '.';
import { ServiceTypes } from './constants';
import * as styled from './styled';

export const Audio: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  const ConnectionSettings = () => {
    switch (values.connection.serviceType) {
      case 'stream':
        return <AudioStream />;
      case 'clip':
        return <AudioClip />;
      case 'tuner':
        return <AudioTuner />;
      default:
        return null;
    }
  };

  const serviceType = ServiceTypes.find((t) => t.value === values.connection.serviceType);

  return (
    <styled.MediaType>
      <h3>Connection Settings</h3>
      <FormikSelect
        label="Service Type"
        name="connection.serviceType"
        options={ServiceTypes}
        value={serviceType}
      />
      {ConnectionSettings()}
    </styled.MediaType>
  );
};
