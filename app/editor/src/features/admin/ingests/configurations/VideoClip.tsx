import { FormikCheckbox, FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import { TimeZones } from './constants';
import * as styled from './styled';

export const VideoClip: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  useTooltips();

  React.useEffect(() => {
    if (!values.configuration.fileName) {
      setFieldValue('configuration.fileName', '{schedule.Name}.mp4');
    }
  });

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);

  return (
    <styled.IngestType>
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        value={timeZone}
      />
      <FormikText label="Format" name="configuration.format" tooltip="Format of the clip" />
      <p>Use "{'{schedule.Name}'}.mp4" to name the file with the schedule name.</p>
      <FormikText
        label="Output File Name"
        name="configuration.fileName"
        tooltip="File name and output format"
        placeholder="{schedule.Name}.mp4"
      />
      <FormikText
        label="Copy Arguments"
        name="configuration.copy"
        tooltip="Copy command arguments"
        placeholder="-c:v copy -c:a copy"
      />
      <FormikText
        label="Frame Rate"
        name="configuration.frameRate"
        value={values.configuration.frameRate}
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
        onChange={(e) => {
          setFieldValue('configuration.keepChecking', e.currentTarget.checked);
        }}
      />
      <FormikCheckbox
        label="Throw error on missing files"
        name="configuration.throwOnMissingFile"
        tooltip="The service will throw an error if the capture file is not found or is missing data"
        onChange={(e) => {
          setFieldValue('configuration.throwOnMissingFile', e.currentTarget.checked);
        }}
      />
    </styled.IngestType>
  );
};
