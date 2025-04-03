import { useFormikContext } from 'formik';
import moment from 'moment';
import * as React from 'react';
import { useLookupOptions } from 'store/hooks';
import {
  FieldSize,
  filterEnabledOptions,
  FormikDatePicker,
  FormikSelect,
  FormikText,
  getSourceOptions,
  IOptionItem,
  Row,
  TimeInput,
} from 'tno-core';

import { IContentForm } from './interfaces';

interface IImageSectionProps {}

/** Contains form field in a layout specific to the image snippet. */
export const ImageSection: React.FunctionComponent<IImageSectionProps> = (props) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [{ sources, mediaTypeOptions }] = useLookupOptions();

  const [sourceOptions, setSourceOptions] = React.useState<IOptionItem[]>([]);

  React.useEffect(() => {
    setSourceOptions(getSourceOptions(sources));
  }, [sources]);

  return (
    <Row>
      <FormikSelect
        name="sourceId"
        label="Media Outlet"
        width={FieldSize.Big}
        value={sourceOptions.find((mt) => mt.value === values.sourceId) ?? ''}
        onChange={(newValue: any) => {
          if (!!newValue) {
            const source = sources.find((ds) => ds.id === newValue.value);
            setFieldValue('sourceId', newValue.value);
            setFieldValue('otherSource', source?.code ?? '');
            if (!!source?.licenseId) setFieldValue('licenseId', source.licenseId);
            if (!!source?.mediaTypeId) setFieldValue('mediaTypeId', source.mediaTypeId);
          }
        }}
        options={filterEnabledOptions(sourceOptions, values.sourceId)}
        required={!values.otherSource || values.otherSource !== ''}
        isDisabled={!!values.tempSource}
      />
      <FormikSelect
        name="mediaTypeId"
        value={mediaTypeOptions.find((mt) => mt.value === values.mediaTypeId) ?? ''}
        label="Media Type"
        width={FieldSize.Small}
        options={mediaTypeOptions}
        required
      />
      <FormikDatePicker
        name="publishedOn"
        label="Published On"
        required
        autoComplete="false"
        width={FieldSize.Medium}
        selectedDate={!!values.publishedOn ? moment(values.publishedOn).toString() : undefined}
        value={!!values.publishedOn ? moment(values.publishedOn).format('MMM D, yyyy') : ''}
        onChange={(date) => {
          if (!!values.publishedOnTime) {
            const hours = values.publishedOnTime?.split(':');
            if (!!hours && !!date) {
              date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
            }
          }
          setFieldValue('publishedOn', moment(date).format('MMM D, yyyy HH:mm:ss'));
        }}
      />
      <TimeInput
        name="publishedOnTime"
        label="Time"
        disabled={!values.publishedOn}
        width="7em"
        value={!!values.publishedOn ? values.publishedOnTime : ''}
        placeholder={!!values.publishedOn ? values.publishedOnTime : 'HH:MM:SS'}
        onChange={(e) => {
          const date = new Date(values.publishedOn);
          const hours = e.target.value?.split(':');
          if (!!hours && !!e.target.value && !e.target.value.includes('_')) {
            date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
            setFieldValue('publishedOn', moment(date.toISOString()).format('MMM D, yyyy HH:mm:ss'));
          }
        }}
      />
      <FormikText name="page" label="Page" />
    </Row>
  );
};
