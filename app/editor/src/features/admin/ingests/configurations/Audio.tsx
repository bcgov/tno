import { FormikSelect } from 'components/formik';
import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { Show } from 'tno-core';

import { AudioClip, AudioStream, AudioTuner } from '.';
import { serviceTypes } from './constants';
import * as styled from './styled';

export const Audio: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();

  const ServiceTypes = serviceTypes(values.ingestType?.name);
  const serviceType = ServiceTypes.find((t) => t.value === values.configuration.serviceType);

  return (
    <styled.IngestType>
      <FormikSelect
        label="Service Type"
        name="configuration.serviceType"
        options={ServiceTypes}
        value={serviceType}
        onChange={(newValue: any) => {
          // If an audio ingest, set the configuration other args.
          if (values.id === 0 && !values.configuration.otherArgs && newValue.value === 'stream') {
            setFieldValue('configuration.otherArgs', '-acodec mp3 -ab 257k');
          }
        }}
      />
      <Show visible={values.configuration.serviceType === 'stream'}>
        <AudioStream />
      </Show>
      <Show visible={values.configuration.serviceType === 'clip'}>
        <AudioClip />
      </Show>
      <Show visible={values.configuration.serviceType === 'tuner'}>
        <AudioTuner />
      </Show>
    </styled.IngestType>
  );
};
