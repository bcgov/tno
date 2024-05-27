import parse from 'html-react-parser';
import { IFilterSettingsModel } from 'tno-core';

export const formatSearch = (text: string, filter: IFilterSettingsModel) => {
  // Check if filter is undefined or null
  if (!filter) {
    return parse(text);
  }

  let tempText = text;
  let parseText = () => {
    if (filter.search) return filter.search;
    else return '';
  };

  const searchWords = parseText()
    .split(' e')
    .filter((word) => word); // Filter out empty words
  if (searchWords.length === 0) {
    // If there are no valid search words, return the original text parsed
    return parse(text);
  }

  searchWords.forEach((word) => {
    const regex = new RegExp(word, 'gi');
    // remove duplicates found only want unique matches, this will be varying capitalization
    const matches = text.match(regex)?.filter((v, i, a) => a.indexOf(v) === i) ?? [];
    // text.match included in replace in order to keep the proper capitalization
    // When there is more than one match, this indicates there will be varying capitalization. In this case we
    // have to iterate through the matches and do a more specific replace in order to keep the words capitalization
    if (matches.length > 1) {
      matches.forEach((match, i) => {
        let multiMatch = new RegExp(`${matches[i]}`);
        tempText = tempText.replace(multiMatch, `<b>${match}</b>`);
      });
    } else if (matches.length === 1) {
      // in this case there will only be one match, so we can just insert the first match
      tempText = tempText.replace(regex, `<b>${matches[0]}</b>`);
    }
  });

  // Check if filter.boldKeywords is undefined or null
  if (!filter.boldKeywords) return parse(text);
  return parse(tempText);
};
