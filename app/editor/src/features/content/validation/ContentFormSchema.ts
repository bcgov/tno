import { date, number, object, string } from 'yup';

export const ContentFormSchema = object().shape({
  // TODO: Either 'otherSource' or 'sourceId' is required.
  otherSource: string().required('Source is a required field.'),
  // TODO: sourceId: number().required('Source is a required field.'),
  productId: number().required('Product designation is a required field.'),
  publishedOn: date().required('Published On is a required field.'),
  // TODO: Summary should not be empty.
  summary: string().required('Summary is a required field.'),
  // TODO: Headline should not be empty.
  headline: string().required('Headline is a required field.'),
  tone: number().required('Tone is a required field.'),
  // TODO: validation for print content.
});
