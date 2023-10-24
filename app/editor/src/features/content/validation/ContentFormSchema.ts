import { ContentTypeName } from 'tno-core';
import { array, date, number, object, string } from 'yup';

import { IContentForm } from '../form/interfaces';

export const ContentFormSchema = object<IContentForm>().shape(
  {
    productId: number().required('Product is required.').notOneOf([0], 'Product is required.'),
    sourceId: number().when('tempSource', (value: number[]) => {
      if (value[0] === undefined)
        return number().required('Either source or other source is required.');
      return number();
    }),
    efforts: string().when('contentType', (value: string[]) => {
      if (value[0] === ContentTypeName.AudioVideo) {
        return number().when('efforts', (efforts: number[]) => {
          if (!efforts[0] || efforts[0] <= 0) {
            return number().moreThan(0, 'Total minutes are required.');
          }
          return number();
        });
      }
      return number();
    }),
    tempSource: string().when('sourceId', (value: string[]) => {
      if (value[0] === undefined)
        return string().required('Either source or other source is required.');
      return string();
    }),
    publishedOn: date().required('Published On is a required field.'),
    // TODO: Summary should not be empty.
    summary: string().when('contentType', (value: string[]) => {
      if (value[0] === ContentTypeName.AudioVideo || value[0] === ContentTypeName.Image) {
        return number().when('productId', (value: number[]) => {
          // Summary is not a required field when content is tagged as News Radio or Events product
          // TODO: This will break eventually because of hardcoded values.
          if (value[0] !== 4 && value[0] !== 9)
            return string().trim().required('Summary is a required field.');
          return string();
        });
      }
      return string();
    }),
    body: string().when('contentType', (value: string[]) => {
      if (value[0] !== ContentTypeName.AudioVideo && value[0] !== ContentTypeName.Image)
        return string().trim().required('Body is a required field.');
      return string();
    }),
    // TODO: Headline should not be empty.
    headline: string().required('Headline is a required field.'),
    tonePools: string().when('contentType', (values: string[]) => {
      if (values[0] !== ContentTypeName.Image) return array().min(1, 'Tone is a required field.');
      return array();
    }),
    // TODO: validation for print content.
    section: string().when('contentType', (value: string[]) => {
      if (value[0] === ContentTypeName.PrintContent)
        return string().trim().required('Section is a required field.');
      return string();
    }),
    byline: string().when('contentType', (value: string[]) => {
      if (value[0] === ContentTypeName.PrintContent)
        return string().trim().required('Byline is a required field.');
      return string();
    }),
  },
  [['sourceId', 'tempSource']],
);
