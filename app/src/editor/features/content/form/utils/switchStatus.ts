import { ContentStatusName } from 'tno-core';

/**
 * Changes the status based on the current status.
 * Note that ideally when checked=false it would return to the prior value, however presently this isn't possible without additional state management.
 * @param checked Whether the 'Publish' checkbox is checked.
 * @param status The current status of the content.
 * @returns The new status based on the checked value.
 */
export const switchStatus = (checked: boolean, status: ContentStatusName) => {
  switch (status) {
    case ContentStatusName.Published:
      return checked ? status : ContentStatusName.Unpublish;
    case ContentStatusName.Unpublish:
    case ContentStatusName.Unpublished:
      return checked ? ContentStatusName.Publish : status;
    case ContentStatusName.Draft:
    case ContentStatusName.Publish:
    default:
      return checked ? ContentStatusName.Publish : ContentStatusName.Draft;
  }
};
