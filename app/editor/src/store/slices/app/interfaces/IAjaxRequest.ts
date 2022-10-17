/**
 * Interface for AJAX requests.
 */
export interface IAjaxRequest {
  /** The URL for the AJAX request. */
  url: string;
  /** A way to group related requests. */
  group: string[];
  /** Whether this request should run silently in the background. */
  isSilent: boolean;
}
