import { FormikCheckbox, FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import { useFormikHelpers } from 'hooks/formik';
import React from 'react';
import { Row } from 'tno-core';

import { TimeZones } from './constants';
import * as styled from './styled';

export const AudioClip: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  const { applyPlaceholder } = useFormikHelpers();
  useTooltips();

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
        tooltip="If more than one ingest is capturing files for a source, then filter by the filename.  Generally the schedule name and file extension (i.e. Morning.mp3)."
      />
      <p>Use "{'{schedule.Name}'}.mp3" to name the file with the schedule name.</p>
      <FormikText
        label="Output File Name"
        name="configuration.fileName"
        tooltip="File name and output format"
        placeholder="{schedule.Name}.mp3"
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
      />
      <FormikCheckbox
        label="Throw error on missing files"
        name="configuration.throwOnMissingFile"
        tooltip="The service will throw an error if the capture file is not found or is missing data"
      />
    </styled.IngestType>
  );
};
