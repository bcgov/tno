import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { FormikSelect } from 'tno-core';

import { VideoClip, VideoRPi, VideoStream, VideoTuner } from '.';
import { serviceTypes } from './constants';
import * as styled from './styled';

export const Video: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();

  let ServiceTypes = serviceTypes(values.ingestType?.name);
  const ConnectionSettings = () => {
    switch (values.configuration.serviceType) {
      case 'stream':
        return <VideoStream />;
      case 'clip':
        return <VideoClip />;
      case 'tuner':
        return <VideoTuner />;
      case 'RPi':
        return <VideoRPi />;
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
        onChange={(newValue: any) => {
          setFieldValue('configuration', {
            serviceType: newValue.value,
            import: values.configuration.import,
            post: values.configuration.post,
          });
        }}
      />
      <hr />
      {ConnectionSettings()}
    </styled.IngestType>
  );
};
