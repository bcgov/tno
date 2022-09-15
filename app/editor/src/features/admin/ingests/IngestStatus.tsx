import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import moment from 'moment';
import React from 'react';
import { Button, ButtonVariant, FieldSize, formatDate, IColProps, Row, Text } from 'tno-core';

import * as styled from './styled';

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
  useTooltips();

  const getStatus = () => {
    if (!values.schedules.length) return 'No Service Schedule';
    if (!values.lastRanOn || !values.isEnabled || values.schedules.every((s) => !s.isEnabled))
      return 'Not Running';

    const lastDelay = moment();
    const lastRanOn = moment(values.lastRanOn).add(5, 'minutes');
    return lastRanOn.isValid() && lastRanOn >= lastDelay ? 'Running' : 'Sleeping';
  };

  return (
    <styled.IngestStatus {...props}>
      <Text label="Status" name="lastRanOn" disabled value={getStatus()} />
      <FormikText
        label="Last Run On"
        name="lastRanOn"
        disabled
        formatter={(value) => formatDate(value, 'YYYY-MM-DD h:mm:ss a')}
      />
      <Row alignItems="flex-end">
        <FormikText
          label="Failure Limit"
          name="retryLimit"
          tooltip="Maximum number of failure before the service goes to sleep"
          type="number"
          width={FieldSize.Tiny}
        />
        <FormikText
          label="Failures"
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
    </styled.IngestStatus>
  );
};
