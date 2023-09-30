import { useFormikContext } from 'formik';
import React from 'react';
import { FormikSelect, IIngestModel } from 'tno-core';

import { VideoClip, VideoHDMI, VideoRPi, VideoStream, VideoTuner } from '.';
import { serviceTypes } from './constants';
import { ImportContent } from './ImportContent';
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
      case 'HDMI':
        return <VideoHDMI />;
      default:
        return null;
    }
  };

  const serviceType = ServiceTypes.find((t) => t.value === values.configuration.serviceType);

  return (
    <styled.IngestType>
      <ImportContent />
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
