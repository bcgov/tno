import * as yup from 'yup';

export const TopicScoreRuleSchema = yup.object().shape({
  id: yup.number().required().min(0),
  sourceId: yup.number().positive('Required').integer().required('Required'),
  section: yup.string().optional(),
  pageMin: yup.string().optional(),
  pageMax: yup.string().optional(),
  hasImage: yup.boolean().optional(),
  characterMin: yup.number().optional().typeError('Invalid number'),
  characterMax: yup.number().optional().typeError('Invalid number'),
  timeMin: yup.string().optional().length(8, 'Invalid format'),
  timeMax: yup.string().optional().length(8, 'Invalid format'),
  score: yup.number().required().min(0).typeError('Invalid number'),
  sortOrder: yup.number().required().min(0).typeError('Invalid number'),
});
