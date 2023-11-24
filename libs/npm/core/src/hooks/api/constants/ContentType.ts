export enum ContentType {
  /** Content contains audio/video. */
  AudioVideo = 0,
  /** Print content is stories published by newspapers in traditional media. */
  PrintContent = 1,
  /** Image files and front page images of newspapers. */
  Image = 2,
  /** Text based content, which can contain files.
   * Used for internet based content with links. */
  Story = 3,
}
