export enum ContentStatus {
  /**
   * Content will not be published.
   */
  Draft = 0,
  /**
   * Content added to queue to publish.
   */
  Publish = 1,
  /**
   * Content has been published.
   */
  Published = 2,
  /**
   * Content has been requested to be unpublished.
   */
  Unpublish = 3,
  /**
   * Content has been unpublished.
   */
  Unpublished = 4,
}
