export enum ProductRequestStatusName {
  /**
   * User is not requesting anything.
   */
  NA = 'NA',
  /**
   * Sends a request to be subscribed to the linked product.
   */
  RequestSubscription = 'RequestSubscription',
  /**
   * Sends a request to cancel the subscription to the linked product.
   */
  RequestUnsubscribe = 'RequestUnsubscribe',
}
