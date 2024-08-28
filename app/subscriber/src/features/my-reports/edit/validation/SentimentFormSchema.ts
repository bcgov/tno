import { array, number, object } from 'yup';

export const sentimentFormSchema = object().shape({
  tonePools: array().of(
    object().shape({
      value: number().required('Tone value is required'),
    }),
  ),
});
