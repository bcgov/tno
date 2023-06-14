/** this function will trim the content text to a given wordcount as well as remove any html tags that are present. */
export const trimWords = (content: string, wordCount: number) => {
  const trimString = content.split(/\s+/, wordCount);
  return trimString
    .join(' ')
    .replace(/<\/?[^>]+(>|$)/g, '')
    .concat('...');
};
