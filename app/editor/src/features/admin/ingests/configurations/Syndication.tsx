import { useFormikContext } from 'formik';
import React from 'react';
import { FormikCheckbox, FormikSelect, FormikText, IIngestModel } from 'tno-core';

import { TimeZones } from './constants';
import { ImportContent } from './ImportContent';
import * as styled from './styled';

export const Syndication: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);

  return (
    <styled.IngestType>
      <ImportContent />
      <FormikText label="Syndication Feed URL" name="configuration.url" required />
      <FormikCheckbox
        label="Custom RSS/ATOM"
        name="configuration.customFeed"
        tooltip="If the feed doesn't follow the standardized rules it is a custom feed."
      />
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        value={timeZone}
      />
      <FormikText
        label="Username"
        name="configuration.username"
        tooltip="Configure in the source connection"
      />
      <FormikText
        label="Password"
        name="configuration.password"
        tooltip="Configure in the source connection"
        type="password"
        autoComplete="off"
      />
      <FormikCheckbox
        label="Fetch Content Body Separately"
        name="configuration.fetchContent"
        tooltip="Whether content body is located remotely"
        onChange={(e) => {
          setFieldValue('configuration.fetchContent', e.currentTarget.checked);
        }}
      />
    </styled.IngestType>
  );
};
