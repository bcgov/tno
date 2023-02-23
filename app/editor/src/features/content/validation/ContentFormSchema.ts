import { ContentTypeName } from 'hooks';
import { date, number, object, string } from 'yup';

export const ContentFormSchema = object().shape({
  // TODO: Either 'otherSource' or 'sourceId' is required.
  otherSource: string().required('Source is a required field.'),
  // TODO: sourceId: number().required('Source is a required field.'),
  productId: number().required('Product designation is a required field.'),
  publishedOn: date().required('Published On is a required field.'),
  // TODO: Summary should not be empty.
  // summary: string().required('Summary is a required field.'),
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
  tonePools: number().when('contentType', {
    is: (value: ContentTypeName) => value !== ContentTypeName.Image,
    then: number().required('Tone is a required field.').typeError('A numeric tone value is required.'),
  }),
  // TODO: validation for print content.
});
