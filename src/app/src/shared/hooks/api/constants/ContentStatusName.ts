export enum ContentStatusName {
  /**
   * Content will not be published.
   */
  Draft = 'Draft',
  /**
   * Content added to queue to publish.
   */
  Publish = 'Publish',
  /**
   * Content has been published.
   */
  Published = 'Published',
  /**
   * Content has been requested to be unpublished.
   */
  Unpublish = 'Unpublish',
  /**
   * Content has been unpublished.
   */
  Unpublished = 'Unpublished',
}
