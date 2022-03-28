import { Col, Row } from 'components/flex';
import { Checkbox, IOptionItem } from 'components/form';
import { FormikCheckbox, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { ActionMeta } from 'react-select';
import { useLookup } from 'store/hooks';
import { getSortableOptions } from 'utils';

import { defaultSchedule } from './constants';
import { Connection } from './media-types';
import * as styled from './styled';

interface IDataSourceDetailsProps {}

export const DataSourceDetails: React.FC<IDataSourceDetailsProps> = () => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const [lookups] = useLookup();

  const mediaTypes = getSortableOptions(lookups.mediaTypes);
  const licenses = getSortableOptions(lookups.licenses);

  const handleMediaTypeChange = (newValue: unknown, actionMeta: ActionMeta<unknown>) => {
    // Change so that the connection settings can display the correct form.
    const option = newValue as IOptionItem;
    const mediaType = lookups.mediaTypes.find((mt) => mt.id === option.value);
    setFieldValue('mediaType', mediaType);
  };

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const mediaType = lookups.mediaTypes.find((mt) => mt.id === values.mediaTypeId);
    setFieldValue('mediaType', mediaType);
  }, [lookups.mediaTypes, setFieldValue, values.mediaTypeId, values.mediaType]);

  return (
    <styled.DataSourceDetails alignContent="flex-start" alignItems="flex-start" flex="1">
      <h2>Details</h2>
      <Row>
        <Col>
          <FormikText label="Name" name="name" required />
          <FormikText label="Abbreviation" name="code" required placeholder="A unique code" />
          <FormikText label="Common Call" name="shortName" />
          <FormikTextArea label="Description" name="description" />
          <FormikSelect
            label="Media Type"
            name="mediaTypeId"
            options={mediaTypes}
            required
            onChange={handleMediaTypeChange}
          />
          <FormikSelect label="License" name="licenseId" options={licenses} required />
          <FormikSelect
            label="Parent Data Source"
            name="parentId"
            options={[]}
            placeholder="optional"
          />
          <Connection />
        </Col>
        <Col>
          <Checkbox
            label="Has Service Schedule"
            name="hasSchedule"
            value={true}
            checked={!!values.schedules.length}
            onChange={(e) => {
              const checked = e.target.checked;
              if (checked && !values.schedules.length)
                setFieldValue('schedules.0', defaultSchedule);
              else if (!checked) setFieldValue('schedules', []);
            }}
          />
          <FormikCheckbox label="Enabled" name="enabled" />
          <FormikCheckbox label="CBRA" name="inCBRA" />
          <FormikCheckbox label="TV Archive" name="inTVArchive" />
          <FormikCheckbox label="Use in Analysis" name="inAnalysis" />
          <FormikCheckbox label="Use in Event of the Day" name="inEoD" />
        </Col>
      </Row>
    </styled.DataSourceDetails>
  );
};
