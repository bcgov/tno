import { ContentTypeName } from 'tno-core';
import { array, date, number, object, string } from 'yup';

export const ContentFormSchema = object().shape(
  {
    productId: number().required('Product is required.').notOneOf([0], 'Product is required.'),
    sourceId: number().when('tempSource', (value: (number | undefined)[]) => {
      if (value[0] === undefined)
        return number().required('Either source or other source is required.');
      return number();
    }),
    tempSource: string().when('sourceId', (value: (string | undefined)[]) => {
      if (value[0] === undefined)
        return string().required('Either source or other source is required.');
      return string();
    }),
    publishedOn: date().required('Published On is a required field.'),
    // TODO: Summary should not be empty.
    summary: string().when('contentType', (value: string[]) => {
      if (value[0] === ContentTypeName.Snippet)
        return string().trim().required('Summary is a required field.');
      return string();
    }),
    body: string().when('contentType', (value: string[]) => {
      if (value[0] !== ContentTypeName.Snippet && value[0] !== ContentTypeName.Image)
        return string().trim().required('Summary is a required field.');
      return string();
    }),
    // TODO: Headline should not be empty.
    headline: string().required('Headline is a required field.'),
    tonePools: string().when('contentType', (value: string[]) => {
      if (value[0] !== ContentTypeName.Image) return array().min(1, 'Tone is a required field.');
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
