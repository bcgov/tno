import {
  actionTypeDefaults,
  buildDefaultActionPrompt,
  CHAT_ACTION_PROMPT_PREFIX,
  LEGACY_CHAT_ACTION_PROMPT_PREFIX,
} from '../constants/defaultAutomationProfile';
import { type IAutomationRuleActionModel } from '../interfaces';

/**
 * Apply the new action type's default prompt/confirmation when the current values have not been
 * customized (blank or still equal to another type's default).
 */
export const syncActionDefaults = (
  action: IAutomationRuleActionModel,
  actionType: string,
  useChatCompletions: boolean = false,
): IAutomationRuleActionModel => {
  const defaults = actionTypeDefaults[actionType];
  if (!defaults) return { ...action, actionType };

  // Default prompts exist in multiple shapes: plain, chat-wrapped (News Story + Actions
  // prefix), and the legacy chat wrapper (recognized so older prompts still resync).
  const allPrompts = Object.values(actionTypeDefaults).flatMap((d) => [
    d.prompt,
    `${CHAT_ACTION_PROMPT_PREFIX}${d.prompt}`,
    `${LEGACY_CHAT_ACTION_PROMPT_PREFIX}${d.prompt}`,
  ]);
  const allConfirmations = Object.values(actionTypeDefaults).map((d) => d.confirmationStatement);
  const promptIsDefault =
    !action.prompt || action.prompt === '<p><br></p>' || allPrompts.includes(action.prompt);
  const confirmationIsDefault =
    !action.confirmationStatement || allConfirmations.includes(action.confirmationStatement);

  return {
    ...action,
    actionType,
    prompt: promptIsDefault
      ? buildDefaultActionPrompt(actionType, useChatCompletions)
      : action.prompt,
    confirmationStatement: confirmationIsDefault
      ? defaults.confirmationStatement
      : action.confirmationStatement,
  };
};
