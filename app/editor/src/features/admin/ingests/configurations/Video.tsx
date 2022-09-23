import { FormikSelect } from 'components/formik';
import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import { VideoClip, VideoStream, VideoTuner } from '.';
import { ServiceTypes } from './constants';
import * as styled from './styled';

export const Video: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  const ConnectionSettings = () => {
    switch (values.configuration.serviceType) {
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

  const serviceType = ServiceTypes.find((t) => t.value === values.configuration.serviceType);

  return (
    <styled.IngestType>
      <FormikSelect
        label="Service Type"
        name="configuration.serviceType"
        options={ServiceTypes}
        value={serviceType}
      />
      {ConnectionSettings()}
    </styled.IngestType>
  );
};
