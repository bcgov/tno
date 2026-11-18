/** Selects all checkboxes in a given list of checkboxes
 * @param checkboxes - list of checkboxes to select
 * @param checked - whether to check or uncheck the checkboxes
 */
export const selectAllCheckboxes = (checkboxes: NodeListOf<HTMLInputElement>, checked: boolean) => {
  checkboxes.forEach((checkbox) => {
    checkbox.checked = checked;
  });
};
