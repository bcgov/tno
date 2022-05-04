import { FieldSize } from 'components/form';
import { FormikText } from 'components/formik';
import { useFormikContext } from 'formik';
import { IDataSourceModel } from 'hooks/api-editor';
import React from 'react';
import { Button, ButtonVariant } from 'tno-core';
import { Row } from 'tno-core/dist/components/flex';

import * as styled from './styled';

export const DataSourceStatus: React.FC = (props) => {
  const { setFieldValue } = useFormikContext<IDataSourceModel>();

  return (
    <styled.DataSourceStatus alignItems="flex-start">
      <FormikText label="Status" name="lastRanOn" disabled placeholder="Unknown" />
      <FormikText label="Last Run On" name="lastRanOn" disabled />
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
