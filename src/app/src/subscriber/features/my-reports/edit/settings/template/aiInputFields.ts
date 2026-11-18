/** Keep the default keys aligned with ReportAISectionGenerator.DefaultInputFields. */
export const aiInputFields = [
  { value: 'headline', label: 'Headline', description: 'The story headline.' },
  { value: 'source', label: 'Source', description: 'The publication or broadcaster.' },
  { value: 'mediaType', label: 'Media type', description: 'The story’s media type.' },
  { value: 'series', label: 'Series', description: 'The program or series name.' },
  {
    value: 'publishedOn',
    label: 'Published date',
    description: 'Publication date and time in UTC.',
  },
  { value: 'byline', label: 'Byline', description: 'The story’s byline.' },
  { value: 'contributor', label: 'Contributor', description: 'The contributor’s name.' },
  {
    value: 'summary',
    label: 'Summary',
    description:
      'The content analysis summary, or the story summary when no analysis summary is available. Omitted when Article text is selected and has a value.',
  },
  {
    value: 'keyFacts',
    label: 'Key facts',
    description: 'Facts extracted by content analysis, when available.',
  },
  {
    value: 'entities',
    label: 'People and organizations',
    description: 'Names extracted by content analysis, when available.',
  },
  {
    value: 'quotes',
    label: 'Quotes',
    description: 'Statements and speakers extracted by content analysis, when available.',
  },
  {
    value: 'body',
    label: 'Article text',
    description:
      'When selected, sends the full article or approved transcript instead of the summary. If empty, uses the summary when selected. When unselected, article text is still used if no summary is available.',
  },
];

export const defaultAIInputFields = aiInputFields
  .filter((field) => field.value !== 'body')
  .map((field) => field.value);
