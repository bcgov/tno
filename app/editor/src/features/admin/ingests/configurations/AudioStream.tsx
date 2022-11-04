import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import { Languages, TimeZones } from './constants';
import * as styled from './styled';

export const AudioStream: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  useTooltips();

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);
  const language = Languages.find((t) => t.value === values.configuration.language);

  return (
    <styled.IngestType>
      <FormikText
        label="Stream URL"
        name="configuration.url"
        tooltip="URL to the source stream"
        required
      />
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        value={timeZone}
      />
      <FormikSelect
        label="Language"
        name="configuration.language"
        options={Languages}
        value={language}
      />
      <FormikText label="Format" name="configuration.format" tooltip="Format of the stream" />
      <p>Use "{'{schedule.Name}'}.mp3" to name the file with the schedule name.</p>
      <FormikText
        label="Output File Name"
        name="configuration.fileName"
        tooltip="File name and output format"
        placeholder="{schedule.Name}.mp3"
      />
      <FormikText
        label="Volume"
        name="configuration.volume"
        tooltip="Volume in percent or dB (1 = 100%)"
        placeholder="1"
      />
      <FormikText
        label="Other Arguments"
        name="configuration.otherArgs"
        tooltip="Any other arguments to pass to the command"
        placeholder="-acodec mp3 -ab 257k"
      />
    </styled.IngestType>
  );
};
