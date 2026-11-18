export enum ProductRequestStatus {
  /**
   * User is not requesting anything.
   */
  NA = 0,
  /**
   * Sends a request to be subscribed to the linked product.
   */
  RequestSubscription = 1,
  /**
   * Sends a request to cancel the subscription to the linked product.
   */
  RequestCancelSubscription = 2,
}
