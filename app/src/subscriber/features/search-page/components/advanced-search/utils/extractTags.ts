export const extractTags = (text: string) => {
  const regex = /\[(.*?)\]/;
  const match = regex.exec(text);

  if (match && match[1]) {
    // The content between square brackets is captured in group 1
    const contentString = match[1];

    // Split the content string by commas and trim whitespace
    const contentArray = contentString.split(',').map((item) => item.trim().toUpperCase());

    return contentArray;
  }

  return [];
};
