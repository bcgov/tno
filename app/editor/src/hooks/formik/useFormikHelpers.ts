import { useFormikContext } from 'formik';

/**
 * Provides helper functions for formik.
 * @returns Class containing helper functions.
 */
export const useFormikHelpers = <TFormikContext>() => {
  const { setFieldValue } = useFormikContext<TFormikContext>();

  /**
   * Apply the placeholder value to the input.
   * @param e Mouse event on input.
   */
  const applyPlaceholder = (e: React.MouseEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    if (e.ctrlKey) {
      setFieldValue(e.currentTarget.name, e.currentTarget.attributes['placeholder' as any].value);
    }
  };

  return { applyPlaceholder };
};
