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
} from 'tno-core';

import { ImportMigrationType } from './constants';
import { ImportContent } from './ImportContent';
import * as styled from './styled';

export const DbMigration: React.FC = (props) => {
  const { values, setFieldValue } = useFormikContext<IIngestModel>();

  return (
    <styled.IngestType>
      <ImportContent />
      <Row gap="1rem">
        <Col flex="1">
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
        </Col>
        <Col flex="1" gap="0.5rem">
          <FormikCheckbox name="configuration.publishedOnly" label="Published Content Only" />
          <FormikCheckbox name="configuration.forceUpdate" label="Force Update" />
        </Col>
      </Row>
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
              required={!values.configuration.offsetHours}
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
      <Row gap="1rem">
        <Show
          visible={
            ![ImportMigrationType.Historic, ImportMigrationType.All].includes(
              values.configuration.importMigrationType,
            )
          }
        >
          <Col flex="1">
            <FormikText
              label="Migration offset in hours"
              name="configuration.offsetHours"
              type="number"
              min="0"
              width="10ch"
              size={5}
              value={values.configuration.offsetHours ?? ''}
              onChange={(e) => {
                const value = e.currentTarget.value;
                const offset = value && !isNaN(Number(value)) ? Number(value) : undefined;
                setFieldValue('configuration.offsetHours', offset);
              }}
            />
          </Col>
        </Show>
        <Col flex="1">
          <FormikText
            label="Import Delay in seconds"
            name="configuration.importDelayMs"
            type="number"
            min="0"
            width="10ch"
            size={5}
            value={
              values.configuration.importDelayMs ? values.configuration.importDelayMs / 1000 : ''
            }
            onChange={(e) => {
              const value = e.currentTarget.value;
              const delay = value && !isNaN(Number(value)) ? Number(value) * 1000 : undefined;
              setFieldValue('configuration.importDelayMs', delay);
            }}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 1">
          <FormikText
            label="Creation Date of Last Imported Item"
            disabled
            name="creationDateOfLastItem"
            value={values.creationDateOfLastItem ?? ''}
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
