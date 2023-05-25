import { useFormikContext } from 'formik';
import React from 'react';
import { ConnectionTypeName, FormikTextArea, IConnectionModel } from 'tno-core';

import {
  AWSConfiguration,
  AzureConfiguration,
  DBConfiguration,
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
    case ConnectionTypeName.Database:
      return <DBConfiguration />;
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
