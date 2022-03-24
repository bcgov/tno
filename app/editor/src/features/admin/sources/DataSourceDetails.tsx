import { Col } from 'components/flex';
import { FormikCheckbox, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { Section } from 'components/section';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { getSortableOptions } from 'utils';

import { defaultSource } from './constants';
import * as styled from './styled';

interface IDataSourceDetailsProps {
  values?: IDataSourceModel;
}

export const DataSourceDetails: React.FC<IDataSourceDetailsProps> = ({
  values = defaultSource,
}) => {
  const [lookups] = useLookup();

  const mediaTypes = getSortableOptions(lookups.mediaTypes);
  const licenses = getSortableOptions(lookups.licenses);

  return (
    <styled.DataSourceDetails alignContent="flex-start" alignItems="flex-start">
      <Col>
        <FormikText label="Name" name="name" required />
        <FormikText label="Abbreviation" name="code" required placeholder="A unique code" />
        <FormikText label="Common Call" name="code" />
        <FormikTextArea label="Description" name="description" />
        <FormikSelect label="Media Type" name="mediaTypeId" options={mediaTypes} required />
        <FormikSelect label="License" name="licenseId" options={licenses} required />
        <FormikSelect
          label="Parent Data Source"
          name="parentId"
          options={[]}
          placeholder="optional"
        />
        <Section>
          <h3>Connection Settings</h3>
          <FormikText label="URL" name="connection.url" value={values.connection.url} />
          <FormikText
            label="username"
            name="connection.username"
            value={values.connection.username}
          />
          <FormikText
            label="password"
            name="connection.password"
            value={values.connection.password}
            type="password"
            autoComplete="off"
          />
        </Section>
      </Col>
      <Col>
        <FormikCheckbox label="Enabled" name="enabled" />
        <FormikCheckbox label="CBRA" name="cbra" />
        <FormikCheckbox label="TV Archive" name="tvArchive" />
        <FormikCheckbox label="Use in Analysis" name="useInAnalysis" />
        <FormikCheckbox label="Use in Event of the Day" name="useInEoD" />
      </Col>
    </styled.DataSourceDetails>
  );
};
