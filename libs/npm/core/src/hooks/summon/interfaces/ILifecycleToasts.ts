/**
 * used by the useSummon hook.
 * loadingToast is the message to display while the api request is pending. This toast is cancelled when the request is completed.
 * successToast is displayed when the request is completed successfully.
 * errorToast is displayed when the request fails for any reason. By default this will return an error from axios.
 */
export interface ILifecycleToasts {
  loadingToast?: () => React.ReactText;
  successToast?: () => React.ReactText;
  errorToast?: () => React.ReactText;
}
