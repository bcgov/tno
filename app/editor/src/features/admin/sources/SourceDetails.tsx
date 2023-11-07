import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import {
  FieldSize,
  filterEnabledOptions,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  getSortableOptions,
  getUserOptions,
  OptionItem,
} from 'tno-core';
import { Col } from 'tno-core/dist/components/flex';

import { TimeZones } from '../ingests/configurations/constants';
import { ISourceForm } from './interfaces';
import * as styled from './styled';

interface ISourceDetailsProps {}

const SourceDetails: React.FC<ISourceDetailsProps> = () => {
  const { values, setFieldValue } = useFormikContext<ISourceForm>();
  const [lookups] = useLookup();

  const users = getUserOptions(lookups.users);
  const licenses = getSortableOptions(lookups.licenses, values.licenseId);
  const products = getSortableOptions(lookups.products, values.productId, [
    new OptionItem('None', ''),
  ]);

  React.useEffect(() => {
    const license = lookups.licenses.find((mt) => mt.id === values.licenseId);
    setFieldValue('license', license);
  }, [lookups.licenses, setFieldValue, values.licenseId, values.license]);

  React.useEffect(() => {
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
          isClearable={false}
        />
        <FormikSelect
          label="Owner"
          name="ownerId"
          tooltip="The user that manages this content"
          value={users.find((o) => o.value === values.ownerId) ?? ''}
          options={filterEnabledOptions(users)}
        />
        <FormikSelect
          label="Product Override"
          name="productId"
          tooltip="The product designation the source content will be assigned (overrides the value in the ingest)"
          value={products.find((o) => o.value === values.productId) ?? ''}
          options={products}
        />
        <FormikSelect
          label="Product Search Group"
          name="productSearchGroupId"
          tooltip="The field is used to help group media sources by their product."
          value={products.find((o) => o.value === values.productSearchGroupId) ?? ''}
          options={products}
        />
        <FormikSelect
          label="Timezone Override"
          name="configuration.timeZone"
          tooltip="Timezone of the source (overrides the value in the ingest)"
          value={TimeZones.find((o) => o.value === values.configuration.timeZone) ?? ''}
          onChange={(newValue) => {
            const option = newValue as OptionItem;
            setFieldValue('configuration.timeZone', option ? option.value : '');
          }}
          options={TimeZones}
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
        <FormikCheckbox label="Automatically transcribe when saved" name="autoTranscribe" />
        <FormikCheckbox label="Disable transcript requests" name="disableTranscribe" />
      </Col>
    </styled.SourceDetails>
  );
};

export default SourceDetails;
