/**
 * The message to show for a failed filter AI request: the API's own message when it sent one.
 * @param error The error the request rejected with.
 * @returns A message for the user.
 */
export const getFilterAIErrorMessage = (error: any): string =>
  error?.response?.data?.error ?? error?.data?.error ?? error?.message ?? 'The AI request failed.';
