import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  formatDate,
  FormikDatePicker,
  FormikText,
  IIngestModel,
  Row,
  TimeInput,
  useFormikHelpers,
} from 'tno-core';

import * as styled from './styled';

export const DbMigration: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const { applyPlaceholder } = useFormikHelpers();

  const minMigrationIngestSpanInDays = 7;
  const maxMigrationIngestSpanInYears = 1;

  const maxEndDate = React.useMemo(() => {
    let dateTimeNow = moment();
    let returnVal;
    if (values.configuration.importDateStart) {
      const startDatePlusMaxSpan = moment(values.configuration.importDateStart).add(
        maxMigrationIngestSpanInYears,
        'year',
      );
      returnVal = startDatePlusMaxSpan > dateTimeNow ? dateTimeNow : startDatePlusMaxSpan;
    } else {
      returnVal = dateTimeNow;
    }
    return returnVal.toDate();
  }, [values.configuration.importDateStart]);

  const minEndDate = React.useMemo(() => {
    let dateTimeNow = moment();
    let returnVal;
    if (values.configuration.importDateStart) {
      const startDatePlusMinSpan = moment(values.configuration.importDateStart).add(
        minMigrationIngestSpanInDays,
        'day',
      );
      returnVal = startDatePlusMinSpan > dateTimeNow ? dateTimeNow : startDatePlusMinSpan;
    } else {
      returnVal = moment().subtract(maxMigrationIngestSpanInYears, 'year');
    }
    return returnVal.toDate();
  }, [values.configuration.importDateStart]);

  const minStartDate = React.useMemo(() => {
    let returnVal;
    if (values.configuration.importDateEnd) {
      returnVal = moment(values.configuration.importDateEnd).subtract(
        maxMigrationIngestSpanInYears,
        'year',
      );
    } else {
      returnVal = moment().subtract(maxMigrationIngestSpanInYears, 'year');
    }
    return returnVal.toDate();
  }, [values.configuration.importDateEnd]);

  const maxStartDate = React.useMemo(() => {
    let returnVal;
    if (values.configuration.importDateEnd) {
      returnVal = moment(values.configuration.importDateEnd).subtract(
        minMigrationIngestSpanInDays,
        'day',
      );
    } else {
      returnVal = moment().subtract(minMigrationIngestSpanInDays, 'day');
    }
    return returnVal.toDate();
  }, [values.configuration.importDateEnd]);

  return (
    <styled.IngestType>
      <Row>
        <Col>
          Max Ingest window is&nbsp;{maxMigrationIngestSpanInYears} Year(s). Min Ingest window
          is&nbsp;{minMigrationIngestSpanInDays} Day(s)
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 1">
          <FormikDatePicker
            name="importDateStart"
            label="Import Start Date"
            autoComplete="false"
            width={FieldSize.Medium}
            selectedDate={
              !!values.configuration.importDateStart
                ? moment(values.configuration.importDateStart).toString()
                : undefined
            }
            value={
              !!values.configuration.importDateStart
                ? moment(values.configuration.importDateStart).format('MMM D, yyyy')
                : undefined
            }
            minDate={minStartDate}
            maxDate={maxStartDate}
            onChange={(date) => {
              if (!!values.configuration.importDateStartTime) {
                const hours = values.configuration.importDateStartTime?.split(':');
                if (!!hours && !!date) {
                  date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
                }
              }
              setFieldValue(
                'configuration.importDateStart',
                moment(date).format('YYYY-MM-DD h:mm:ss a'),
              );
            }}
          />
        </Col>
        <Col>
          <TimeInput
            name="importDateStartTime"
            label="Time"
            disabled={!values.configuration.importDateStart}
            width="7em"
            value={
              !!values.configuration.importDateStart
                ? formatDate(values.configuration.importDateStart, 'HH:mm:ss')
                : undefined
            }
            placeholder={
              !!values.configuration.importDateStart
                ? formatDate(values.configuration.importDateStart, 'HH:mm:ss')?.toString()
                : 'HH:MM:SS'
            }
            onChange={(e) => {
              const date = new Date(values.configuration.importDateStart);
              const hours = e.target.value?.split(':');
              if (!!hours && !!e.target.value && !e.target.value.includes('_')) {
                date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
                setFieldValue(
                  'configuration.importDateStart',
                  moment(date.toISOString()).format('YYYY-MM-DD h:mm:ss a'),
                );
              }
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 1">
          <FormikDatePicker
            name="importDateEnd"
            label="Import End Date"
            autoComplete="false"
            width={FieldSize.Medium}
            selectedDate={
              !!values.configuration.importDateEnd
                ? moment(values.configuration.importDateEnd).toString()
                : undefined
            }
            value={
              !!values.configuration.importDateEnd
                ? moment(values.configuration.importDateEnd).format('MMM D, yyyy')
                : ''
            }
            minDate={minEndDate}
            maxDate={maxEndDate}
            onChange={(date) => {
              if (!!values.configuration.importDateEndTime) {
                const hours = values.configuration.importDateEndTime?.split(':');
                if (!!hours && !!date) {
                  date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
                }
              }
              setFieldValue(
                'configuration.importDateEnd',
                moment(date).format('YYYY-MM-DD h:mm:ss a'),
              );
            }}
          />
        </Col>
        <Col>
          <TimeInput
            name="importDateEndTime"
            label="Time"
            disabled={!values.configuration.importDateEnd}
            width="7em"
            value={
              !!values.configuration.importDateEnd
                ? formatDate(values.configuration.importDateEnd, 'HH:mm:ss')
                : ''
            }
            placeholder={
              !!values.configuration.importDateEnd
                ? formatDate(values.configuration.importDateEnd, 'HH:mm:ss')?.toString()
                : 'HH:MM:SS'
            }
            onChange={(e) => {
              const date = new Date(values.configuration.importDateEnd);
              const hours = e.target.value?.split(':');
              if (!!hours && !!e.target.value && !e.target.value.includes('_')) {
                date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
                setFieldValue(
                  'configuration.importDateEnd',
                  moment(date.toISOString()).format('YYYY-MM-DD h:mm:ss a'),
                );
              }
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 1">
          <FormikText
            label="Creation Date of Last Imported Item"
            disabled
            name="configuration.creationDateOfLastImport"
            value={values.configuration.creationDateOfLastImport}
            tooltip="The Creation Date of the last item imported from the Source System"
            formatter={(value) => formatDate(value, 'YYYY-MM-DD h:mm:ss a')}
          />
        </Col>
        <Col>
          <label>&nbsp;</label>
          <Button
            tooltip="Reset"
            variant={ButtonVariant.danger}
            disabled={!values.configuration.creationDateOfLastImport}
            onClick={() => {
              setFieldValue('configuration.creationDateOfLastImport', null);
            }}
          >
            <FaTrash />
          </Button>
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 1">
          <FormikText
            label="Migration offset in hours"
            name="configuration.migrationTimeOffsetInHours"
            value={values.configuration.migrationTimeOffsetInHours}
            type="number"
            placeholder="2"
            min="0"
            size={5}
            onClick={applyPlaceholder}
          />
        </Col>
      </Row>
    </styled.IngestType>
  );
};
