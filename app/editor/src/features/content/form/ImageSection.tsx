import { useFormikContext } from 'formik';
import { ISourceModel } from 'hooks';
import moment from 'moment';
import * as React from 'react';
import { useLookup } from 'store/hooks';
import { filterEnabled } from 'store/hooks/lookup/utils';
import { FieldSize, FormikDatePicker, FormikSelect, IOptionItem, Row, TimeInput } from 'tno-core';
import { getSortableOptions } from 'utils';

import { IContentForm } from './interfaces';

interface IImageSectionProps {
  sourceOptions: IOptionItem[];
  sources: ISourceModel[];
  productOptions: IOptionItem[];
}

/** Contains form field in a layout specific to the image snippet. */
export const ImageSection: React.FunctionComponent<IImageSectionProps> = ({
  sourceOptions,
  sources,
  productOptions,
}) => {
  const [categoryOptions, setCategoryOptions] = React.useState<IOptionItem[]>([]);
  const [{ categories }] = useLookup();
  const { values, setFieldValue } = useFormikContext<IContentForm>();

  React.useEffect(() => {
    setCategoryOptions(getSortableOptions(categories));
  }, [categories]);

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
            if (!!source?.productId) setFieldValue('productId', source.productId);
          }
        }}
        options={filterEnabled(sourceOptions, values.sourceId).filter(
          (x) =>
            x.label.includes('(TC)') ||
            x.label.includes('(PROVINCE)') ||
            x.label.includes('(GLOBE)') ||
            x.label.includes('(POST)') ||
            x.label.includes('(SUN)'),
        )}
        required={!values.otherSource || values.otherSource !== ''}
        isDisabled={!!values.tempSource}
      />
      <FormikSelect
        name="productId"
        value={productOptions.find((mt) => mt.value === values.productId) ?? ''}
        label="Product"
        width={FieldSize.Small}
        options={productOptions}
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
      <FormikSelect
        name="categories"
        label="Topic"
        width={FieldSize.Medium}
        options={filterEnabled(
          categoryOptions,
          !!values.categories?.length ? values.categories[0].id : null,
        )}
        value={
          !!values.categories?.length
            ? categoryOptions.find((c) => c.value === values.categories[0].id)
            : []
        }
        onChange={(e: any) => {
          // only supports one at a time right now
          let value;
          if (!!e?.value) {
            value = categories.find((c) => c.id === e.value);
          }
          setFieldValue('categories', !!value ? [value] : []);
        }}
      />
    </Row>
  );
};
