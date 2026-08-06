import { type IAutomationRuleActionModel } from '../interfaces';

/**
 * Apply the scoring objective to the action, replacing the `{objective}` keyword in the prompt
 * and confirmation statement. When the objective changes, previously substituted markers and
 * phrases are updated to the new objective.
 */
export const applyObjective = (
  action: IAutomationRuleActionModel,
  objective: string,
): IAutomationRuleActionModel => {
  const next = objective.trim();
  const prev = (action.objective ?? '').trim();

  const replaceTokens = (text: string): string => {
    if (!text || !next) return text;
    let result = text.split('{objective}').join(next);
    if (prev && prev !== next) {
      result = result
        .split(`[SCORE ${prev}:`)
        .join(`[SCORE ${next}:`)
        .split(`[SELECT ${prev}:`)
        .join(`[SELECT ${next}:`)
        .split(`{candidates:${prev}}`)
        .join(`{candidates:${next}}`)
        .split(`"${prev}" objective`)
        .join(`"${next}" objective`);
    }
    return result;
  };

  return {
    ...action,
    objective: objective,
    prompt: replaceTokens(action.prompt),
    confirmationStatement: replaceTokens(action.confirmationStatement),
  };
};
