import { FormikText } from 'components/formik';
import { getIn, useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { IMetricModel, ISourceModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { FieldSize } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';

import * as styled from './styled';

export const SourceMetrics: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<ISourceModel>();
  const [{ metrics }] = useLookup();
  const { field } = useNamespace('metrics');

  const formValues: IMetricModel[] = getIn(values, 'metrics', []);

  React.useEffect(() => {
    // Make sure the available metrics are all included in the data source.
    if (values.metrics.length !== metrics.length) {
      const existingMetrics = [...values.metrics];
      metrics.forEach((sm) => {
        const metric: IMetricModel | undefined = values.metrics.find((vm) => vm.id === sm.id);
        if (metric === undefined) {
          existingMetrics.push(sm);
        }
      });
      setFieldValue('metrics', existingMetrics);
    }
  }, [setFieldValue, metrics, values.metrics]);

  const options = formValues.map((m, i) => {
    return (
      <Row key={m.id} nowrap>
        <div className="rem-label">
          <label>{m.name}</label>
        </div>
        <FormikText label="Reach" name={field('reach', i)} width={FieldSize.Small} type="number" />
        <FormikText
          label="Earned Media"
          name={field('earned', i)}
          width={FieldSize.Small}
          type="number"
        />
      </Row>
    );
  });

  return <styled.SourceMetrics>{options}</styled.SourceMetrics>;
};
