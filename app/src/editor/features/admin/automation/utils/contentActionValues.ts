import { type IActionModel, ValueType } from 'tno-core';

import {
  type IAutomationAction,
  type IAutomationActionDescriptor,
  type IAutomationDefinition,
  type IAutomationValueSource,
} from '../designer/interfaces';

/** The catalog field kind whose control follows the picked content action's own value type. */
export const CONTENT_ACTION_VALUE_KIND = 'contentActionValue';

/**
 * Whether a value source actually supplies something. An empty literal, a blank reference, or a
 * blank template is the editor's 'not filled in yet' shape, not a value. Mirrors the server-side
 * validator so the editor and the API agree on what counts as missing.
 */
export const hasValueSource = (value?: IAutomationValueSource | null): boolean => {
  if (!value) return false;
  if (value.from?.trim()) return true;
  if (value.template?.trim()) return true;
  if (value.literal === undefined || value.literal === null) return false;
  return `${value.literal}`.trim() !== '';
};

/** The content action an action stamps, when it stamps one. */
export const findContentAction = (
  action: IAutomationAction,
  contentActions: IActionModel[],
): IActionModel | undefined =>
  action.contentAction ? contentActions.find((a) => a.id === action.contentAction) : undefined;

/**
 * Whether the action stamps a content action that records a value of its own (Commentary's
 * timeout, for example) rather than a plain yes/no flag — those need a value to stamp.
 */
export const requiresContentActionValue = (
  action: IAutomationAction,
  descriptor: IAutomationActionDescriptor | undefined,
  contentActions: IActionModel[],
): boolean => {
  if (!descriptor?.fields.some((field) => field.kind === CONTENT_ACTION_VALUE_KIND)) return false;
  const picked = findContentAction(action, contentActions);
  return !!picked && picked.valueType !== ValueType.Boolean;
};

/**
 * Every stamp missing the value its content action stores, labelled 'step › action (flag)' for
 * the save-time message.
 */
export const findMissingContentActionValues = (
  definition: IAutomationDefinition,
  descriptors: IAutomationActionDescriptor[],
  contentActions: IActionModel[],
): string[] =>
  definition.steps.flatMap((step) =>
    step.actions
      .filter((action) => {
        const descriptor = descriptors.find((d) => d.type === action.type);
        return (
          requiresContentActionValue(action, descriptor, contentActions) &&
          !hasValueSource(action.value)
        );
      })
      .map((action) => {
        const descriptor = descriptors.find((d) => d.type === action.type);
        const label = action.name || descriptor?.label || action.type;
        return `${step.name || 'step'} › ${label} (${
          findContentAction(action, contentActions)?.name
        })`;
      }),
  );
