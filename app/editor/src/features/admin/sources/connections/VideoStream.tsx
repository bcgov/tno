import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import ReactTooltip from 'react-tooltip';

import { Languages, TimeZones } from './constants';
import * as styled from './styled';

export const VideoStream: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();

  React.useEffect(() => {
    ReactTooltip.rebuild();
    if (!values.connection.fileName) {
      setFieldValue('connection.fileName', '{schedule.Name}.mp4');
    }
  });

  const timeZone = TimeZones.find((t) => t.value === values.connection.timeZone);
  const language = Languages.find((t) => t.value === values.connection.language);

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
        label="Stream URL"
        name="connection.url"
        tooltip="URL to the source stream"
        required
      />
      <FormikText
        label="Format"
        name="connection.format"
        tooltip="Format of the stream"
        placeholder="mp4"
      />
      <FormikText
        label="File Name"
        name="connection.fileName"
        tooltip="File name and output format"
        placeholder="{schedule.Name}.mp4"
      />
      <FormikText
        label="Frame Rate"
        name="connection.frameRate"
        value={values.connection.frameRate}
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
      <FormikSelect
        label="Language"
        name="connection.language"
        options={Languages}
        defaultValue={language}
      />
    </styled.MediaType>
  );
};
