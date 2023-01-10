import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { FormikSelect, Show } from 'tno-core';

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
          setFieldValue('configuration', {
            serviceType: newValue.value,
            import: values.configuration.import,
            post: values.configuration.post,
            otherArgs:
              values.id === 0 && !values.configuration.otherArgs && newValue.value === 'stream'
                ? '-acodec mp3 -ab 257k'
                : undefined,
          });
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
