import * as yup from 'yup';

export const TopicSchema = yup.object().shape({
  id: yup.number().required().min(0),
  name: yup.string().required('Name is required').min(2, 'Name is required'),
  description: yup.string().optional(),
  topicType: yup.string().required(),
  sortOrder: yup.number().required().min(0),
});
