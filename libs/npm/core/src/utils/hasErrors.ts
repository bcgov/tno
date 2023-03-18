/**
 * Function used to check if there are any errors in the formik form for a given set of fields.
 * @param errors The Formik errors
 * @param fields The fields to check for errors
 * @returns True if any of the fields have errors
 */
export const hasErrors = (errors: any, fields: string[]) => {
  return fields.some((p) => !!errors[p]);
};
