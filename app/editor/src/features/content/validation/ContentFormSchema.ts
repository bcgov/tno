import { date, number, object, string } from 'yup';

export const ContentFormSchema = object().shape({
  source: string().required('Source is a required field.'),
  mediaTypeId: number().required('Media Type is a required field.'),
  seriesId: number().required('Series is a required field.'),
  publishedOn: date().required('Published On is a required field.'),
  summary: string().required('Summary is a required field.'),
  // add toning
});
