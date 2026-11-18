import React from 'react';
import { toast } from 'react-toastify';

/**
 * Hook that provides common properties and functions to control a validation toast.
 * @returns Hook properties to control a toast.
 */
export const useTabValidationToasts = (message?: string) => {
  const [showValidationToast, setShowValidationToast] = React.useState(false);

  React.useEffect(() => {
    if (showValidationToast)
      toast.error(
        !!message ? message : 'Please refer to the highlighted tab and fix the validation errors.',
      );
  }, [message, showValidationToast]);

  return {
    showValidationToast,
    setShowValidationToast,
  };
};
