import { FieldSize, IOptionItem, OptionItem } from 'components/form';
import { FormikCheckbox, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { ActionMeta } from 'react-select';
import { useLookup } from 'store/hooks';
import { useDataSources } from 'store/hooks/admin';
import { Col, Row } from 'tno-core/dist/components/flex';
import { getDataSourceOptions, getSortableOptions } from 'utils';

import { DataSourceActions, DataSourceStatus } from '.';
import * as styled from './styled';

interface IDataSourceDetailsProps {}

export const DataSourceDetails: React.FC<IDataSourceDetailsProps> = () => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const [lookups] = useLookup();
  const [{ dataSources }, api] = useDataSources();

  const [init, setInit] = React.useState(true);
  const [sources, setSources] = React.useState(
    getDataSourceOptions(dataSources, [new OptionItem('No Parent', 0)]),
  );

  const mediaTypes = getSortableOptions(lookups.mediaTypes);
  const licenses = getSortableOptions(lookups.licenses);
  const dataLocations = getSortableOptions(lookups.dataLocations);

  React.useEffect(() => {
    if (init && !dataSources.length) {
      api.findDataSources().then((page) => {
        setSources(getDataSourceOptions(page.items, [new OptionItem('No Parent', 0)]));
      });
      setInit(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dataSources.length, init]);

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const mediaType = lookups.mediaTypes.find((mt) => mt.id === values.mediaTypeId);
    setFieldValue('mediaType', mediaType);
  }, [lookups.mediaTypes, setFieldValue, values.mediaTypeId, values.mediaType]);

  const handleMediaTypeChange = (newValue: unknown, actionMeta: ActionMeta<unknown>) => {
    // Change so that the connection settings can display the correct form.
    const option = newValue as IOptionItem;
    const mediaType = lookups.mediaTypes.find((mt) => mt.id === option.value);
    setFieldValue('mediaType', mediaType);
  };

  return (
    <styled.DataSourceDetails alignItems="center">
      <Row nowrap>
        <Row>
          <Col>
            <FormikText label="Name" name="name" required />
            <FormikText label="Abbreviation" name="code" required placeholder="A unique code" />
            <FormikText label="Common Call" name="shortName" />
            <FormikTextArea label="Description" name="description" />
          </Col>
          <Col>
            <FormikSelect
              label="Media Type"
              name="mediaTypeId"
              options={mediaTypes}
              required
              onChange={handleMediaTypeChange}
            />
            <FormikSelect label="License" name="licenseId" options={licenses} required />
            <FormikSelect
              label="Data Location"
              name="dataLocationId"
              options={dataLocations}
              required
            />
            <FormikSelect
              label="Parent Data Source"
              name="parentId"
              options={sources}
              placeholder="optional"
              width={FieldSize.Medium}
            />
          </Col>
          <Col>
            <FormikCheckbox label="Enabled" name="isEnabled" />
            <DataSourceActions />
          </Col>
        </Row>
        <DataSourceStatus />
      </Row>
    </styled.DataSourceDetails>
  );
};
