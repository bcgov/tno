export const hasErrors = (errors: any, props: string[]) => {
  return props.some((p) => !!errors[p]);
};
