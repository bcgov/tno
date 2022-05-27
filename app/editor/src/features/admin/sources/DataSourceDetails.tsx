import { FieldSize, IOptionItem, OptionItem } from 'components/form';
import { FormikCheckbox, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { ActionMeta } from 'react-select';
import ReactTooltip from 'react-tooltip';
import { useLookup } from 'store/hooks';
import { useDataSources } from 'store/hooks/admin';
import { Col } from 'tno-core/dist/components/flex';
import { getDataSourceOptions, getSortableOptions } from 'utils';

import { DataSourceActions } from '.';
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
    const contentType = lookups.contentTypes.find((mt) => mt.id === values.contentTypeId);
    setFieldValue('contentType', contentType);
  }, [lookups.contentTypes, setFieldValue, values.contentTypeId, values.contentType]);

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const mediaType = lookups.mediaTypes.find((mt) => mt.id === values.mediaTypeId);
    setFieldValue('mediaType', mediaType);
  }, [lookups.mediaTypes, setFieldValue, values.mediaTypeId, values.mediaType]);

  React.useEffect(() => {
    ReactTooltip.rebuild();
  });

  const handleMediaTypeChange = (newValue: unknown, actionMeta: ActionMeta<unknown>) => {
    // Change so that the connection settings can display the correct form.
    const option = newValue as IOptionItem;
    const mediaType = lookups.mediaTypes.find((mt) => mt.id === option.value);
    setFieldValue('mediaType', mediaType);
  };

  return (
    <styled.DataSourceDetails>
      <Col>
        <FormikText
          label="Legal Name"
          name="name"
          tooltip="The full name of the data source"
          required
        />
        <FormikText
          label="Code"
          name="code"
          tooltip="A unique code to identify this data source"
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
          label="Media Type"
          name="mediaTypeId"
          tooltip="The type of media this data source provides"
          options={mediaTypes}
          required
          onChange={handleMediaTypeChange}
        />
        <FormikSelect
          label="License"
          name="licenseId"
          tooltip="Manage the length of time content will be stored"
          options={licenses}
          required
        />
        <FormikSelect
          label="Data Location"
          name="dataLocationId"
          tooltip="The physical location that data is stored"
          options={dataLocations}
          required
        />
        <FormikSelect
          label="Parent Data Source"
          name="parentId"
          tooltip="Organize related data sources"
          options={sources}
          placeholder={values.connection.serviceType === 'clip' ? 'required' : 'optional'}
          required={values.connection.serviceType === 'clip'}
          width={FieldSize.Medium}
        />
      </Col>
      <Col>
        <FormikCheckbox label="Enabled" name="isEnabled" />
        <DataSourceActions />
      </Col>
    </styled.DataSourceDetails>
  );
};
