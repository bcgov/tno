import * as yup from 'yup';

import { TopicScoreRuleSchema } from './';

export const TopicScoreRulesSchema = yup.array().of(TopicScoreRuleSchema);
