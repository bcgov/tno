import { FieldSize, formatDate, Text } from 'components/form';
import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import moment from 'moment';
import React from 'react';
import { Button, ButtonVariant } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';

import * as styled from './styled';

/**
 * DataSourceStatus component provides a way to display the current status of the data source service.
 * Enables a way to reset the failed attempts.
 * // TODO: Get status from service API instead of static value of 'lastRanOn'.
 * @param props Component properties.
 * @returns DataSourceStatus Component.
 */
export const DataSourceStatus: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IDataSourceModel>();

  const getStatus = () => {
    if (!values.schedules.length) return 'No Service Schedule';
    if (!values.lastRanOn || !values.isEnabled || values.schedules.every((s) => !s.isEnabled))
      return 'Not Running';

    const minDelay =
      Math.min.apply(
        Math,
        values.schedules.map((s) => s.delayMS),
      ) * -1;
    const lastDelay = moment();
    lastDelay.add(minDelay, 'ms');

    const lastRanOn = moment(values.lastRanOn);
    return lastRanOn.isValid() && lastRanOn >= lastDelay ? 'Running' : 'Sleeping';
  };

  return (
    <styled.DataSourceStatus alignItems="flex-start">
      <Text label="Status" name="lastRanOn" disabled value={getStatus()} />
      <FormikText
        label="Last Run On"
        name="lastRanOn"
        disabled
        formatter={(value) => formatDate(value, 'YYYY-MM-DD h:mm:ss a')}
      />
      <Row alignItems="flex-end">
        <FormikText label="Attempts Made" name="failedAttempts" disabled width={FieldSize.Tiny}>
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => {
              setFieldValue('failedAttempts', 0);
            }}
          >
            Reset
          </Button>
        </FormikText>
      </Row>
      <FormikText label="Attempts Limit" name="retryLimit" type="number" width={FieldSize.Tiny} />
    </styled.DataSourceStatus>
  );
};
