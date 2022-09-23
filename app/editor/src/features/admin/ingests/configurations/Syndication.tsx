import { FormikCheckbox, FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';

import { TimeZones } from './constants';
import * as styled from './styled';

export const Syndication: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);
  useTooltips();

  return (
    <styled.IngestType>
      <FormikText
        label="Syndication Feed URL"
        name="configuration.url"
        value={values.configuration.url}
        required
      />
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        tooltip="Timezone of the source"
        options={TimeZones}
        defaultValue={timeZone}
      />
      <FormikText
        label="Username"
        name="configuration.username"
        value={values.configuration.username}
        tooltip="Configure in the source connection"
      />
      <FormikText
        label="Password"
        name="configuration.password"
        value={values.configuration.password}
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
