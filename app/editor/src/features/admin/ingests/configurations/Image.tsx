import { FormikSelect, FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { ConnectionTypeName, IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { useConnections } from 'store/hooks/admin';
import { Show } from 'tno-core';

import { ImageTypes } from './constants';
import * as styled from './styled';

export const Image: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  const [{ connections }, { findAllConnections }] = useConnections();
  useTooltips();

  const [loading, setLoading] = React.useState(true);

  const imageType = ImageTypes.find((t) => t.value === values.configuration.imageType);

  React.useEffect(() => {
    // TODO: This doesn't handle failures after loading the first time.
    if (connections.length === 0 && loading) {
      setLoading(false);
      findAllConnections();
    }
  }, [loading, connections, findAllConnections]);

  return (
    <styled.MediaType>
      <FormikSelect
        label="Image Type"
        name="configuration.imageType"
        options={ImageTypes}
        value={imageType}
      />
      <FormikText
        label="Path to Files"
        name="configuration.path"
        value={values.configuration.path}
      />
      <FormikText
        label="Input File Code"
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
    </styled.MediaType>
  );
};
