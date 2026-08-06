import { type IAutomationRuleActionModel } from '../interfaces';

/**
 * Apply the selected content field to the action, replacing the `{field}` keyword in the prompt
 * and confirmation statement with the field name. When the field changes, previously substituted
 * marker lines and the "the <field> field" phrase are updated to the new field.
 */
export const applyContentField = (
  action: IAutomationRuleActionModel,
  contentField: string | null,
): IAutomationRuleActionModel => {
  const next = contentField ?? '';
  const prev = action.contentField ?? '';

  const replaceTokens = (text: string): string => {
    if (!text || !next) return text;
    let result = text.split('{field}').join(next);
    if (prev && prev !== next) {
      result = result
        .split(`[UPDATE FIELD START:${prev}]`)
        .join(`[UPDATE FIELD START:${next}]`)
        .split(`[UPDATE FIELD END:${prev}]`)
        .join(`[UPDATE FIELD END:${next}]`)
        .split(`the ${prev} field`)
        .join(`the ${next} field`);
    }
    return result;
  };

  return {
    ...action,
    contentField,
    prompt: replaceTokens(action.prompt),
    confirmationStatement: replaceTokens(action.confirmationStatement),
  };
};
