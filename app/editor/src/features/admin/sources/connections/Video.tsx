import { FormikSelect } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';

import { VideoClip, VideoStream, VideoTuner } from '.';
import { ServiceTypes } from './constants';
import * as styled from './styled';

export const Video: React.FC = (props) => {
  const { values } = useFormikContext<IDataSourceModel>();

  const ConnectionSettings = () => {
    switch (values.connection.serviceType) {
      case 'stream':
        return <VideoStream />;
      case 'clip':
        return <VideoClip />;
      case 'tuner':
        return <VideoTuner />;
      default:
        return null;
    }
  };

  const serviceType = ServiceTypes.find((t) => t.value === values.connection.serviceType);

  return (
    <styled.MediaType>
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
