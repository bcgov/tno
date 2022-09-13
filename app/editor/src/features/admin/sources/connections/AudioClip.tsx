import { FormikCheckbox, FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import ReactTooltip from 'react-tooltip';

import { TimeZones } from './constants';
import * as styled from './styled';

export const AudioClip: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();

  React.useEffect(() => {
    ReactTooltip.rebuild();
  });

  const timeZone = TimeZones.find((t) => t.value === values.connection.timeZone);

  return (
    <styled.MediaType>
      <FormikSelect
        label="Timezone"
        name="connection.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        defaultValue={timeZone}
      />
      <FormikText
        label="Format"
        name="connection.format"
        tooltip="Format of the clip"
        placeholder="mp3"
      />
      <FormikText
        label="File Name"
        name="connection.fileName"
        tooltip="File name and output format"
        placeholder="{schedule.Name}.mp3"
      />
      <FormikText
        label="Copy Arguments"
        name="connection.copy"
        tooltip="Copy command arguments"
        placeholder="-c:v copy -c:a copy"
      />
      <FormikText
        label="Volume"
        name="connection.volume"
        tooltip="Volume in percent or dB (1 = 100%)"
        placeholder="1"
      />
      <FormikText
        label="Other Arguments"
        name="connection.otherArgs"
        tooltip="Any other arguments to pass to the command"
      />
      <FormikCheckbox
        label="Retry prior schedules"
        name="connection.keepChecking"
        tooltip="Always check if earlier schedules have successfully generated clips (This is not performant)"
        onChange={(e) => {
          setFieldValue('connection.keepChecking', e.currentTarget.checked);
        }}
      />
      <FormikCheckbox
        label="Throw error on missing files"
        name="connection.throwOnMissingFile"
        tooltip="The service will throw an error if the capture file is not found or is missing data"
        onChange={(e) => {
          setFieldValue('connection.throwOnMissingFile', e.currentTarget.checked);
        }}
      />
    </styled.MediaType>
  );
};
