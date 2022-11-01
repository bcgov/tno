import { FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';
import { ConnectionTypeName, IConnectionModel } from 'hooks';
import React from 'react';

import {
  AWSConfiguration,
  AzureConfiguration,
  FTPConfiguration,
  HTTPConfiguration,
  LocalVolumeConfiguration,
  NASConfiguration,
  SFTPConfiguration,
  SSHConfiguration,
} from '.';

/**
 * Connection configuration settings depending on the connection type.
 * @returns Component.
 */
export const ConnectionConfiguration = () => {
  const { values, setFieldValue } = useFormikContext<IConnectionModel>();

  const [settings, setSettings] = React.useState('{}');

  const connectionType = values.connectionType ?? '';

  React.useEffect(() => {
    setSettings(JSON.stringify(values.configuration ?? '{}'));
  }, [values.configuration]);

  switch (connectionType) {
    case ConnectionTypeName.LocalVolume:
      return <LocalVolumeConfiguration />;
    case ConnectionTypeName.HTTP:
      return <HTTPConfiguration />;
    case ConnectionTypeName.SSH:
      return <SSHConfiguration />;
    case ConnectionTypeName.NAS:
      return <NASConfiguration />;
    case ConnectionTypeName.FTP:
      return <FTPConfiguration />;
    case ConnectionTypeName.SFTP:
      return <SFTPConfiguration />;
    case ConnectionTypeName.Azure:
      return <AzureConfiguration />;
    case ConnectionTypeName.AWS:
      return <AWSConfiguration />;
    default:
      return (
        <FormikTextArea
          name="settings"
          label="Configuration"
          tooltip="Must be valid JSON"
          value={settings}
          onChange={(e) => {
            setSettings(e.currentTarget.value);
          }}
          onBlur={(e) => {
            setFieldValue('configuration', JSON.parse(e.currentTarget.value));
          }}
        />
      );
  }
};
