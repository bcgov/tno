import { FormikCheckbox, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { ISourceModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { Col } from 'tno-core/dist/components/flex';
import { getSortableOptions, getUserOptions } from 'utils';

import { SourceActions } from '.';
import * as styled from './styled';

interface ISourceDetailsProps {}

export const SourceDetails: React.FC<ISourceDetailsProps> = () => {
  const { values, setFieldValue } = useFormikContext<ISourceModel>();
  const [lookups] = useLookup();
  useTooltips();

  const users = getUserOptions(lookups.users);
  const licenses = getSortableOptions(lookups.licenses);

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const license = lookups.licenses.find((mt) => mt.id === values.licenseId);
    setFieldValue('license', license);
  }, [lookups.licenses, setFieldValue, values.licenseId, values.license]);

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const user = lookups.users.find((mt) => mt.id === values.ownerId);
    setFieldValue('owner', user);
  }, [lookups.users, setFieldValue, values.ownerId, values.owner]);

  return (
    <styled.SourceDetails>
      <Col>
        <FormikText label="Legal Name" name="name" tooltip="The full name of the source" required />
        <FormikText
          label="Code"
          name="code"
          tooltip="A unique code to identify this source"
          required
          placeholder="A unique code"
        />
        <FormikText
          label="Friendly Name"
          name="shortName"
          tooltip="A common name used instead of the full legal name"
        />
        <FormikTextArea label="Description" name="description" />
      </Col>
      <Col>
        <FormikSelect
          label="License"
          name="licenseId"
          tooltip="Manage the length of time content will be stored"
          options={licenses}
          required
        />
        <FormikSelect
          label="Owner"
          name="ownerId"
          tooltip="The user that manages this content"
          options={users}
        />
      </Col>
      <Col>
        <FormikCheckbox label="Enabled" name="isEnabled" />
        <SourceActions />
        <FormikCheckbox label="Transcribe when Published" name="autoTranscribe" />
        <FormikCheckbox label="Disable transcript requests" name="disableTranscribe" />
      </Col>
    </styled.SourceDetails>
  );
};
