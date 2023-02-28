import { ContentTypeName } from 'hooks';
import { array, date, number, object, string } from 'yup';

export const ContentFormSchema = object().shape(
  {
    productId: number().required('Product is a required field.').notOneOf([0]),
    sourceId: number().when('tempSource', {
      is: undefined,
      then: number().required('Either source or other source is required.'),
    }),
    tempSource: string().when('sourceId', {
      is: undefined,
      then: string().trim().required('Either source or other source is required.'),
    }),
    publishedOn: date().required('Published On is a required field.'),
    // TODO: Summary should not be empty.
    summary: string().when('contentType', {
      is: ContentTypeName.Snippet,
      then: string().trim().required('Summary is a required field.'),
    }),
    body: string().when('contentType', {
      is: (value: ContentTypeName) =>
        value !== ContentTypeName.Snippet && value !== ContentTypeName.Image,
      then: string().trim().required('Summary is a required field.'),
    }),
    // TODO: Headline should not be empty.
    headline: string().required('Headline is a required field.'),
    tonePools: array().when('contentType', {
      is: (value: ContentTypeName) => value !== ContentTypeName.Image,
      then: array().min(1, 'Tone is a required field.'),
    }),
    // TODO: validation for print content.
    section: string().when('contentType', {
      is: (value: ContentTypeName) => value === ContentTypeName.PrintContent,
      then: string().trim().required('Section is a required field.'),
    }),
    byline: string().when('contentType', {
      is: (value: ContentTypeName) => value === ContentTypeName.PrintContent,
      then: string().trim().required('Byline is a required field.'),
    }),
  },
  [['sourceId', 'tempSource']],
);
