import * as yup from 'yup';

import { maxPageLength } from '../utils/page';

const optionalWholeNumber = yup
  .number()
  .transform((value, original) => (original === '' ? undefined : value))
  .integer('Must be a whole number')
  .min(0, 'Must be 0 or more')
  .typeError('Must be a number')
  .optional();

const time = yup
  .string()
  .optional()
  .test(
    'time',
    'Use HH:MM:SS',
    (value) => !value || /^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$/.test(value),
  );

/** Validation for the rule drawer. Time ranges may wrap midnight, so only pages and characters are ordered. */
export const TopicScoreRuleSchema = yup.object().shape({
  sourceId: yup.number().positive('Required').integer().required('Required'),
  pagePrefix: yup
    .string()
    .optional()
    .matches(/^[a-zA-Z]*$/, 'Letters only'),
  pageMin: optionalWholeNumber.test(
    'page-length',
    `Prefix and number must be at most ${maxPageLength} characters`,
    function (value) {
      return (
        value === undefined || `${this.parent.pagePrefix ?? ''}${value}`.length <= maxPageLength
      );
    },
  ),
  pageMax: optionalWholeNumber
    .test(
      'page-length',
      `Prefix and number must be at most ${maxPageLength} characters`,
      function (value) {
        return (
          value === undefined || `${this.parent.pagePrefix ?? ''}${value}`.length <= maxPageLength
        );
      },
    )
    .test('page-order', 'Must not be less than the minimum', function (value) {
      const min = this.parent.pageMin;
      return value === undefined || min === '' || min === undefined || Number(min) <= value;
    }),
  timeMin: time,
  timeMax: time,
  characterMin: optionalWholeNumber,
  characterMax: optionalWholeNumber.test(
    'character-order',
    'Must not be less than the minimum',
    function (value) {
      const min = this.parent.characterMin;
      return value === undefined || min === '' || min === undefined || Number(min) <= value;
    },
  ),
  score: yup
    .number()
    .transform((value, original) => (original === '' ? undefined : value))
    .required('Required')
    .integer('Must be a whole number')
    .min(0, 'Must be 0 or more')
    .typeError('Must be a number'),
});
