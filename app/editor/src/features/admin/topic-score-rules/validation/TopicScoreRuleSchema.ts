import * as yup from 'yup';

export const TopicScoreRuleSchema = yup.object().shape({
  id: yup.number().required().min(0),
  sourceId: yup.number().positive('Required').integer().required('Required'),
  section: yup.string().optional(),
  pageMin: yup.string().optional(),
  pageMax: yup
    .string()
    .optional()
    .when('pageMin', (pageMin: string, schema: any) => {
      return schema.test({
        test: (pageMax: string) => {
          const riPageMin = pageMin !== undefined ? pageMin.search(/\d*$/) : undefined;
          const riPageMax = pageMax !== undefined ? pageMax.search(/\d*$/) : undefined;
          const rPageMin = riPageMin !== undefined ? Number(pageMin?.slice(riPageMin)) : undefined;
          const rPageMax = riPageMax !== undefined ? Number(pageMax?.slice(riPageMax)) : undefined;
          return rPageMin === undefined || rPageMax === undefined || rPageMin <= rPageMax;
        },
        message: 'Make greater',
      });
    }),
  hasImage: yup.boolean().optional(),
  characterMin: yup.number().optional().typeError('Invalid number'),
  characterMax: yup
    .number()
    .optional()
    .typeError('Invalid number')
    .when('characterMin', (characterMin: number, schema: any) => {
      return schema.test({
        test: (characterMax: number) => {
          return (
            characterMin === undefined || characterMax === undefined || characterMin <= characterMax
          );
        },
        message: 'Make greater',
      });
    }),
  timeMin: yup.string().optional().length(8, 'Invalid format'),
  timeMax: yup
    .string()
    .optional()
    .length(8, 'Invalid format')
    .when('timeMin', (timeMin: string, schema: any) => {
      return schema.test({
        test: (timeMax: string) => {
          const min = Date.parse(`01/01/1900 ${timeMin}`);
          const max = Date.parse(`01/01/1900 ${timeMax}`);
          return timeMin === undefined || timeMax === undefined || min <= max;
        },
        message: 'Make greater',
      });
    }),
  score: yup.number().required().min(0).typeError('Invalid number'),
  sortOrder: yup.number().required().min(0).typeError('Invalid number'),
});
