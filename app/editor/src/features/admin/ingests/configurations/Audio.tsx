import { FormikSelect } from 'components/formik';
import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { Show } from 'tno-core';

import { AudioClip, AudioStream, AudioTuner } from '.';
import { ServiceTypes } from './constants';
import * as styled from './styled';

export const Audio: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  const serviceType = ServiceTypes.find((t) => t.value === values.configuration.serviceType);

  return (
    <styled.MediaType>
      <FormikSelect
        label="Service Type"
        name="configuration.serviceType"
        options={ServiceTypes}
        value={serviceType}
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
    </styled.MediaType>
  );
};
