/**remove any html tags that are present. */
export const stripHtml = (content: string) => {
  return content.replace(/<\/?[^>]+(>|$)/g, '');
};
