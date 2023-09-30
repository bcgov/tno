import { useFormikContext } from 'formik';
import React from 'react';
import { FormikSelect, IIngestModel, Show } from 'tno-core';

import { AudioClip, AudioStream, AudioTuner } from '.';
import { serviceTypes } from './constants';
import { ImportContent } from './ImportContent';
import * as styled from './styled';

export const Audio: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();

  const ServiceTypes = serviceTypes(values.ingestType?.name);
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
      <hr />
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
