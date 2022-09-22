import { FormikCheckbox, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { Col } from 'tno-core/dist/components/flex';
import { getSortableOptions } from 'utils';

import * as styled from './styled';

interface IIngestDetailsProps {}

export const IngestDetails: React.FC<IIngestDetailsProps> = () => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const [lookups] = useLookup();
  useTooltips();

  const sources = getSortableOptions(lookups.sources);
  const products = getSortableOptions(lookups.products);

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
