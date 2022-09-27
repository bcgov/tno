import { FormikCheckbox, FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import { TimeZones } from './constants';
import * as styled from './styled';

export const AudioClip: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  useTooltips();

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);

  return (
    <styled.IngestType>
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        defaultValue={timeZone}
      />
      <FormikText
        label="Format"
        name="configuration.format"
        tooltip="Format of the clip"
        placeholder="mp3"
      />
      <p>Use "{'{schedule.Name}'}.mp3" to name the file with the schedule name.</p>
      <FormikText
        label="File Name"
        name="configuration.fileName"
        tooltip="File name and output format"
        placeholder="{schedule.Name}.mp3"
      />
      <FormikText
        label="Copy Arguments"
        name="configuration.copy"
        tooltip="Copy command arguments"
        placeholder="-c:v copy -c:a copy"
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
      />
      <FormikCheckbox
        label="Retry prior schedules"
        name="configuration.keepChecking"
        tooltip="Always check if earlier schedules have successfully generated clips (This is not performant)"
      />
      <FormikCheckbox
        label="Throw error on missing files"
        name="configuration.throwOnMissingFile"
        tooltip="The service will throw an error if the capture file is not found or is missing data"
      />
    </styled.IngestType>
  );
};
