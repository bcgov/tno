// Function to escape special characters in a string for regex
function escapeRegExp(str: string) {
  return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

export const highlightTerms = (
  body: string,
  highlightedVals: string[],
): (string | JSX.Element)[] => {
  if (typeof body !== 'string' || highlightedVals.length === 0) {
    return [body];
  }

  const regex = new RegExp(
    `(${highlightedVals.map((term) => escapeRegExp(term)).join('|')})`,
    'gi',
  );

  // Split the body into segments based on the regex
  const segments = body.split(regex);

  const result: (string | JSX.Element)[] = segments.map((segment, index) => {
    if (highlightedVals.some((term) => segment.toLowerCase().includes(term.toLowerCase()))) {
      return <b key={`highlighted_${index}`}>{segment}</b>;
    } else {
      return segment;
    }
  });

  return result;
};
