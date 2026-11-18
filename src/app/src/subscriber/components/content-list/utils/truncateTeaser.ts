import { htmlToPlainText } from '@/shared/utils/htmlToPlainText';

export const truncateTeaser = (htmlString: string, charLimit: number): string => {
  const textContent = htmlToPlainText(htmlString);
  return textContent.length > charLimit ? textContent.slice(0, charLimit) + '...' : textContent;
};
