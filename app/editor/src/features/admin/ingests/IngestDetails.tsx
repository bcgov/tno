import {
  FormikCheckbox,
  FormikError,
  FormikSelect,
  FormikText,
  FormikTextArea,
} from 'components/formik';
import { getIn, useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { useDataLocations } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, Row } from 'tno-core';
import { getSortableOptions } from 'utils';

import * as styled from './styled';

interface IIngestDetailsProps {}

export const IngestDetails: React.FC<IIngestDetailsProps> = () => {
  const { values, setFieldValue, setErrors, errors } = useFormikContext<IIngestModel>();
  const [lookups] = useLookup();
  const [{ dataLocations }, { findAllDataLocations }] = useDataLocations();
  useTooltips();

  const [loading, setLoading] = React.useState(true);
  const dataLocationOptions = getSortableOptions(dataLocations);

  const sources = getSortableOptions(lookups.sources);
  const products = getSortableOptions(lookups.products);

  React.useEffect(() => {
    if (loading && dataLocations.length === 0) {
      setLoading(false);
      findAllDataLocations();
    }
  }, [dataLocations, findAllDataLocations, loading]);

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const contentType = lookups.sources.find((mt) => mt.id === values.sourceId);
    setFieldValue('contentType', contentType);
  }, [lookups.sources, setFieldValue, values.sourceId, values.source]);

  React.useEffect(() => {
    // Ensures the connection settings can display the correct form on initial load.
    const product = lookups.products.find((mt) => mt.id === values.productId);
    setFieldValue('product', product);
  }, [lookups.products, setFieldValue, values.productId, values.product]);

  return (
    <styled.IngestDetails>
      <Col>
        <FormikText label="Name" name="name" required />
        <FormikTextArea label="Description" name="description" />
        <p>Choose the location that services will run this ingest.</p>
        <FormikSelect
          label="Locations"
          name="dataLocationId"
          placeholder="Select Location"
          options={dataLocationOptions}
        >
          <Button
            variant={ButtonVariant.secondary}
            disabled={!getIn(values, 'dataLocationId')}
            onClick={(e) => {
              const id = getIn(values, 'dataLocationId');
              const location = dataLocations.find((r) => r.id === id);
              if (location && !values.dataLocations?.some((r) => r.id === id)) {
                setFieldValue('dataLocations', [...values.dataLocations, location]);
                setErrors({ ...errors, dataLocations: undefined });
              }
            }}
          >
            Add
          </Button>
        </FormikSelect>
        <FormikError name="dataLocations" />
        <hr />
        <div className="locations">
          {values.dataLocations?.map((location) => (
            <Row key={location.id}>
              <Col flex="1 1 auto" alignSelf="center">
                {location.name}
              </Col>
              <Col>
                <Button
                  variant={ButtonVariant.danger}
                  onClick={(e) => {
                    setFieldValue(
                      'dataLocations',
                      values.dataLocations?.filter((ur) => ur !== location) ?? [],
                    );
                  }}
                >
                  <FaTrash />
                </Button>
              </Col>
            </Row>
          ))}
        </div>
      </Col>
      <Col>
        <FormikSelect
          label="Source"
          name="sourceId"
          tooltip="The source the ingested content will be assigned"
          options={sources}
          onChange={(newValue: any) => {
            // Use the source.code to set the Kafka topic.
            const source = lookups.sources.find((s) => s.id === newValue.value);
            setFieldValue('topic', source?.code ?? '');
          }}
          required
        />
        <FormikSelect
          label="Product Designation"
          name="productId"
          tooltip="The product designation the ingested content will be assigned"
          options={products}
          required
        />
      </Col>
      <Col>
        <FormikCheckbox label="Enabled" name="isEnabled" />
      </Col>
    </styled.IngestDetails>
  );
};
