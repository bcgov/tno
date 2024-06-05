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
  FormikCheckbox,
  FormikDatePicker,
  FormikRadioGroup,
  FormikText,
  IIngestModel,
  Row,
  Show,
  TimeInput,
  useFormikHelpers,
} from 'tno-core';

import { ImportMigrationType } from './constants';
import { ImportContent } from './ImportContent';
import * as styled from './styled';

export const DbMigration: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();
  const { applyPlaceholder } = useFormikHelpers();

  const minMigrationIngestSpanInDays = 1;
  const maxMigrationIngestSpanInYears = 1;

  const maxEndDate = React.useMemo(() => {
    let dateTimeNow = moment();
    let returnVal;
    if (values.configuration.importDateStart) {
      const startDatePlusMaxSpan = moment(
        values.configuration.importDateStart,
        'YYYY-MM-DD hh:mm:ss a',
      ).add(maxMigrationIngestSpanInYears, 'year');
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
      const startDatePlusMinSpan = moment(
        values.configuration.importDateStart,
        'YYYY-MM-DD hh:mm:ss a',
      ).add(minMigrationIngestSpanInDays, 'day');
      returnVal = startDatePlusMinSpan > dateTimeNow ? dateTimeNow : startDatePlusMinSpan;
    } else {
      returnVal = moment().subtract(maxMigrationIngestSpanInYears, 'year');
    }
    return returnVal.toDate();
  }, [values.configuration.importDateStart]);

  const minStartDate = React.useMemo(() => {
    let returnVal;
    if (values.configuration.importDateEnd) {
      returnVal = moment(values.configuration.importDateEnd, 'YYYY-MM-DD hh:mm:ss a').subtract(
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
      returnVal = moment(values.configuration.importDateEnd, 'YYYY-MM-DD hh:mm:ss a').subtract(
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
      <ImportContent />
      <Col gap="0.5rem">
        <p>
          Max Ingest window is&nbsp;{maxMigrationIngestSpanInYears} Year(s). Min Ingest window
          is&nbsp;{minMigrationIngestSpanInDays} Day(s)
        </p>
        <FormikRadioGroup
          label="Migration Type"
          name="importMigrationType"
          value={values.configuration.importMigrationType ?? ImportMigrationType.Current}
          onChange={(val) => {
            const configuration = {
              ...values.configuration,
              importMigrationType: val.target.value,
              offsetHours:
                val.target.value === ImportMigrationType.Historic ||
                val.target.value === ImportMigrationType.All
                  ? undefined
                  : values.configuration.offsetHours,
              importDateStart:
                val.target.value === ImportMigrationType.Current
                  ? undefined
                  : values.configuration.importDateStart,
              importDateEnd:
                val.target.value === ImportMigrationType.Current
                  ? undefined
                  : values.configuration.importDateEnd,
            };
            setFieldValue('configuration', configuration);
          }}
          options={[
            ImportMigrationType.Historic,
            ImportMigrationType.All,
            ImportMigrationType.Recent,
            ImportMigrationType.Current,
          ]}
          required={true}
        />
        <FormikCheckbox name="configuration.publishedOnly" label="Published Content Only" />
        <FormikCheckbox name="configuration.forceUpdate" label="Force Update" />
      </Col>
      <Show visible={values.configuration.importMigrationType !== ImportMigrationType.Current}>
        <Row>
          <Col>
            <FormikDatePicker
              name="importDateStart"
              label="Import Start Date"
              autoComplete="false"
              width={FieldSize.Medium}
              selectedDate={
                values.configuration.importDateStart
                  ? moment(values.configuration.importDateStart).toLocaleString()
                  : ''
              }
              minDate={minStartDate}
              maxDate={maxStartDate}
              required={values.configuration.importMigrationType === ImportMigrationType.Historic}
              onChange={(date) => {
                if (
                  date &&
                  values.configuration.importDateStart &&
                  moment(date).isValid() &&
                  moment(values.configuration.importDateStart).isValid()
                ) {
                  const importDateStart = moment(values.configuration.importDateStart);
                  date.setHours(
                    importDateStart.hour(),
                    importDateStart.minute(),
                    importDateStart.second(),
                  );
                }

                const value = moment(date).isValid()
                  ? moment(date).format('YYYY-MM-DD HH:mm:ss a')
                  : undefined;
                setFieldValue('configuration.importDateStart', value);
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
                    moment(date.toISOString()).format('YYYY-MM-DD HH:mm:ss a'),
                  );
                }
              }}
            />
          </Col>
          <Col>
            <label>&nbsp;</label>
            <Button
              tooltip="Reset"
              variant={ButtonVariant.danger}
              disabled={!values.configuration.importDateStart}
              onClick={() => {
                setFieldValue('configuration.importDateStart', null);
              }}
            >
              <FaTrash />
            </Button>
          </Col>
        </Row>
        <Row>
          <Col flex="1 1 1">
            <FormikDatePicker
              name="importDateEnd"
              label="Import End Date"
              autoComplete="false"
              width={FieldSize.Medium}
              selectedDate={values.configuration.importDateEnd ?? ''}
              minDate={minEndDate}
              maxDate={maxEndDate}
              required={values.configuration.importMigrationType === ImportMigrationType.Historic}
              onChange={(date) => {
                if (
                  date &&
                  values.configuration.importDateEndTime &&
                  moment(date).isValid() &&
                  moment(values.configuration.importDateEndTime).isValid()
                ) {
                  const importDateEndTime = moment(values.configuration.importDateEndTime);
                  date.setHours(
                    importDateEndTime.hour(),
                    importDateEndTime.minute(),
                    importDateEndTime.second(),
                  );
                }

                const value = moment(date).isValid()
                  ? moment(date).format('YYYY-MM-DD HH:mm:ss a')
                  : undefined;
                setFieldValue('configuration.importDateEnd', value);
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
                  : 'HH:mm:ss'
              }
              onChange={(e) => {
                const date = new Date(values.configuration.importDateEnd);
                const hours = e.target.value?.split(':');
                if (!!hours && !!e.target.value && !e.target.value.includes('_')) {
                  date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
                  setFieldValue(
                    'configuration.importDateEnd',
                    moment(date.toISOString()).format('YYYY-MM-DD HH:mm:ss a'),
                  );
                }
              }}
            />
          </Col>
          <Col>
            <label>&nbsp;</label>
            <Button
              tooltip="Reset"
              variant={ButtonVariant.danger}
              disabled={!values.configuration.importDateEnd}
              onClick={() => {
                setFieldValue('configuration.importDateEnd', null);
              }}
            >
              <FaTrash />
            </Button>
          </Col>
        </Row>
      </Show>

      <Show
        visible={
          ![ImportMigrationType.Historic, ImportMigrationType.All].includes(
            values.configuration.importMigrationType,
          )
        }
      >
        <FormikText
          label="Migration offset in hours"
          name="configuration.offsetHours"
          value={values.configuration.offsetHours}
          type="number"
          min="0"
          width="10ch"
          size={5}
          onClick={applyPlaceholder}
        />
      </Show>
      <Row>
        <Col flex="1 1 1">
          <FormikText
            label="Creation Date of Last Imported Item"
            disabled
            name="creationDateOfLastItem"
            value={values.creationDateOfLastItem}
            tooltip="The Creation Date of the last item imported from the Source System"
            formatter={(value) => formatDate(value, 'YYYY-MM-DD h:mm:ss a')}
          />
        </Col>
        <Col>
          <label>&nbsp;</label>
          <Button
            tooltip="Reset"
            variant={ButtonVariant.danger}
            disabled={!values.creationDateOfLastItem}
            onClick={() => {
              setFieldValue('creationDateOfLastItem', null);
            }}
          >
            <FaTrash />
          </Button>
        </Col>
      </Row>
    </styled.IngestType>
  );
};
