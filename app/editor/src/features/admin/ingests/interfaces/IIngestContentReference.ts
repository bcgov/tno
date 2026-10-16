import { type IContentReferenceFilter, type IContentReferenceModel } from 'tno-core';

/** A content reference with the content imported for it. */
export interface IIngestContentReference extends IContentReferenceModel {
  /** When the ingest discovered the content. */
  createdOn?: string;
  /** The imported content, when it has been imported. */
  contentId?: number;
  /** The headline of the imported content. */
  headline?: string;
}

/** Find content references discovered by an ingest within a date range. */
export interface IIngestContentReferenceFilter extends IContentReferenceFilter {
  /** Discovered (created) on or after this date. */
  createdStartOn?: string;
  /** Discovered (created) before this date. */
  createdEndOn?: string;
}
