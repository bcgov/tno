import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import {} from 'store/hooks/lookup/utils';
import {
  FieldSize,
  filterEnabledOptions,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getSortableOptions,
  getUserOptions,
  ISourceModel,
  OptionItem,
} from 'tno-core';
import { Col } from 'tno-core/dist/components/flex';

import { TimeZones } from '../ingests/configurations/constants';
import * as styled from './styled';

interface ISourceDetailsProps {}

export const SourceDetails: React.FC<ISourceDetailsProps> = () => {
  const { values, setFieldValue } = useFormikContext<ISourceModel>();
  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);
  const [lookups] = useLookup();

  const users = getUserOptions(lookups.users);
  const licenses = getSortableOptions(lookups.licenses);
  const products = getSortableOptions(lookups.products, [new OptionItem('None', undefined)]);

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
          label="Licence"
          name="licenseId"
          tooltip="Manage the length of time content will be stored"
          options={licenses}
          required
        />
        <FormikSelect
          label="Owner"
          name="ownerId"
          tooltip="The user that manages this content"
          options={filterEnabledOptions(users)}
        />
        <FormikSelect
          label="Product Override"
          name="productId"
          tooltip="The product designation the source content will be assigned (overrides the value in the ingest)"
          options={products}
        />
        <FormikSelect
          label="Timezone Override"
          name="configuration.timeZone"
          tooltip="Timezone of the source (overrides the value in the ingest)"
          options={TimeZones}
          value={timeZone}
        />
        <FormikText
          width={FieldSize.Tiny}
          name="sortOrder"
          label="Sort Order"
          type="number"
          className="sort-order"
        />
      </Col>
      <Col>
        <FormikCheckbox label="Enabled" name="isEnabled" />
        <FormikCheckbox label="Use in Topics" name="useInTopics" />
        <FormikCheckbox label="Transcribe when Published" name="autoTranscribe" />
        <FormikCheckbox label="Disable transcript requests" name="disableTranscribe" />
      </Col>
    </styled.SourceDetails>
  );
};
