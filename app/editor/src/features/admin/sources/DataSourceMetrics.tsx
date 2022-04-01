import { FieldSize } from 'components/form';
import { FormikText } from 'components/formik';
import { getIn, useFormikContext } from 'formik';
import { useNamespace } from 'hooks';
import { IDataSourceModel, ISourceMetricModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { Row } from 'tno-core/dist/components/flex';

import * as styled from './styled';

export const DataSourceMetrics: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();
  const [{ sourceMetrics }] = useLookup();
  const { field } = useNamespace('metrics');

  const metrics: ISourceMetricModel[] = getIn(values, 'metrics', []);

  React.useEffect(() => {
    // Make sure the available metrics are all included in the data source.
    if (values.metrics.length !== sourceMetrics.length) {
      const existingMetrics = [...values.metrics];
      sourceMetrics.forEach((sm) => {
        const metric: ISourceMetricModel | undefined = values.metrics.find((vm) => vm.id === sm.id);
        if (metric === undefined) {
          existingMetrics.push(sm);
        }
      });
      setFieldValue('metrics', existingMetrics);
    }
  }, [setFieldValue, sourceMetrics, values.metrics]);

  const options = metrics.map((m, i) => {
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

  return <styled.DataSourceMetrics>{options}</styled.DataSourceMetrics>;
};
