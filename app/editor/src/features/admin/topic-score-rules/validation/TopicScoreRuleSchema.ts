import * as yup from 'yup';

export const TopicScoreRuleSchema = yup.object().shape({
  id: yup.number().required().min(0),
  sourceId: yup.number().positive('Source is required').integer().required('Source is required'),
  section: yup.string().optional(),
  pageMin: yup.number().optional(),
  pageMax: yup.number().optional(),
  hasImage: yup.boolean().optional(),
  characterMin: yup.number().optional(),
  characterMax: yup.number().optional(),
  timeMin: yup.string().optional(),
  timeMax: yup.string().optional(),
  score: yup.number().required().min(0),
  sortOrder: yup.number().required().min(0),
});
