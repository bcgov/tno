export const truncateTeaser = (htmlString: string, charLimit: number): string => {
  const textContent = htmlString.replace(/<[^>]*>/g, '');
  return textContent.length > charLimit ? textContent.slice(0, charLimit) + '...' : textContent;
};
