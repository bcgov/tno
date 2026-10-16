/**
 * A feed in a syndication ingest's ordered 'feeds' configuration.
 * The position of the feed is its priority when duplicate stories are removed (first is highest).
 */
export interface ISyndicationFeed {
  /** The feed URL. */
  url: string;
  /** The source code assigned to stories from this feed, empty to use the ingest's source mapping and source. */
  source?: string;
}
