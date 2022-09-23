import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { ConnectionTypeName, IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { Show } from 'tno-core';

import * as styled from './styled';

export const FrontPage: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  useTooltips();

  return (
    <styled.IngestType>
      <FormikText
        label="Path to Files"
        name="configuration.path"
        value={values.configuration.path}
      />
      <FormikText
        label="File Name Expression (i.e. Code)"
        name="configuration.code"
        value={values.configuration.code}
      />
      <Show visible={values.sourceConnection?.connectionType === ConnectionTypeName.SSH}>
        <FormikText
          label="Hostname"
          name="_.hostname"
          value={values.sourceConnection?.configuration.hostname}
          tooltip="Configure in the source connection"
          disabled
        />
        <FormikText
          label="Username"
          name="_.username"
          value={values.sourceConnection?.configuration.username}
          tooltip="Configure in the source connection"
          disabled
        />
        <FormikText
          label="Password"
          name="_.password"
          value={values.sourceConnection?.configuration.password}
          tooltip="Configure in the source connection"
          type="password"
          disabled
          autoComplete="off"
        />
        <FormikText
          label="Key Filename"
          name="_.keyFileName"
          value={values.sourceConnection?.configuration.keyFileName}
          tooltip="Configure in the source connection"
          disabled
        />
      </Show>
    </styled.IngestType>
  );
};
