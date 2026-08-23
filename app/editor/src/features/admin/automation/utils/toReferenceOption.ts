import { type IOptionItem } from 'tno-core';

import { createOption } from './createOption';

/** Anything the designer offers by id: content actions, LLMs, filters, reports, notifications. */
export interface IReferenceRecord {
  id: number;
  name: string;
  isEnabled?: boolean;
}

/**
 * One picker option for a reference record.
 *
 * A disabled record stays in the list rather than being filtered out: these option arrays double
 * as the label resolver for the value a profile already holds, so dropping the record would blank
 * the picker and hide the fact that the profile still references it. It is labelled and made
 * unselectable instead, so it cannot be chosen for new work.
 */
export const toReferenceOption = (record: IReferenceRecord): IOptionItem =>
  record.isEnabled === false
    ? createOption(`${record.name} (disabled)`, record.id, true)
    : createOption(record.name, record.id);
