import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import { useFormikHelpers } from 'hooks/formik';
import React from 'react';
import { FormikCheckbox, FormikSelect, FormikText, Row } from 'tno-core';

import { TimeZones } from './constants';
import * as styled from './styled';

export const VideoClip: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const { applyPlaceholder } = useFormikHelpers();

  React.useEffect(() => {
    if (!values.configuration.fileName) {
      setFieldValue('configuration.fileName', '{schedule.Name}.mp4');
    }
  });

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);

  return (
    <styled.IngestType>
      <FormikText
        label="Device Hostname"
        name="configuration.hostname"
        value={values.configuration.hostname}
        tooltip="Only devices specifically with this hostname will run this ingest"
      />
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        value={timeZone}
      />
      <FormikText
        label="Capture Filename Filter"
        name="configuration.sourceFile"
        value={values.configuration.sourceFile}
        tooltip="If more than one ingest is capturing files for a source, then filter by the filename.  Generally the schedule name and file extension (i.e. Morning.mkv)."
      />
      <p>Use "{'{schedule.Name}'}.mp4" to name the file with the schedule name.</p>
      <FormikText
        label="Output File Name"
        name="configuration.fileName"
        tooltip="File name and output format"
        placeholder="{schedule.Name}.mp4"
        onClick={applyPlaceholder}
      />
      <FormikText
        label="Copy Arguments"
        name="configuration.copy"
        tooltip="Copy command arguments"
        placeholder="-c:v copy -c:a copy"
        onClick={applyPlaceholder}
      />
      <Row>
        <FormikText label="Format" name="configuration.format" tooltip="Format of the clip" />
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
          onClick={applyPlaceholder}
        />
      </Row>
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
