export const searchInOptions = (callback: (value: string | number | undefined) => void) => [
  {
    id: 'all',
    label: 'All',
    onClick: () => callback('all'),
  },
  {
    id: 'headline',
    label: 'Headline',
    onClick: () => callback('headline'),
  },
  {
    id: 'byline',
    label: 'Byline',
    onClick: () => callback('byline'),
  },
  {
    id: 'story',
    label: 'Story text',
    onClick: () => callback('story'),
  },
];
