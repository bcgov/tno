import { useFormikContext } from 'formik';
import React from 'react';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  formatDate,
  FormikText,
  IColProps,
  IIngestModel,
  Row,
  Text,
} from 'tno-core';

import { getStatus } from './utils';

export interface IIngestStatusProps extends IColProps {}

/**
 * IngestStatus component provides a way to display the current status of the data source service.
 * Enables a way to reset the failed attempts.
 * // TODO: Get status from service API instead of static value of 'lastRanOn'.
 * @param props Component properties.
 * @returns IngestStatus Component.
 */
export const IngestStatus: React.FC<IIngestStatusProps> = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();

  return (
    <Col {...props}>
      <Row justifyContent="center">
        <Text label="Status" name="status" disabled value={getStatus(values)} />
        <FormikText
          label="Last Run On"
          name="lastRanOn"
          disabled
          formatter={(value) => formatDate(value, 'YYYY-MM-DD h:mm:ss a')}
        />
        <FormikText
          label="Failure Limit"
          name="retryLimit"
          tooltip="Maximum number of failure before the service goes to sleep"
          type="number"
          width={FieldSize.Tiny}
        />
        <FormikText
          label="Reset Interval (seconds)"
          name="resetRetryAfterDelayMs"
          tooltip="After hitting the failure limit, the service will auto-reset after this many seconds. Set to [0] for no auto-reset."
          type="number"
          width={FieldSize.Tiny}
          value={!!values.resetRetryAfterDelayMs ? +values.resetRetryAfterDelayMs / 1000 : ''}
          min={1}
          onChange={(e) => {
            const value = Number(e.target.value) * 1000;
            setFieldValue('resetRetryAfterDelayMs', value);
          }}
        />
        <FormikText
          label="Failure Count"
          name="failedAttempts"
          tooltip="Number of sequential failures"
          disabled
          width={FieldSize.Tiny}
        >
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
    </Col>
  );
};
