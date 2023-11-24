import { OptionItem } from '../../components';

export const getReportSectionOrderByOptions = () => {
  return [
    new OptionItem('Published On', 'PublishedOn', true),
    new OptionItem('Media Type', 'MediaType', true),
    new OptionItem('Series', 'Series', true),
    new OptionItem('Source', 'Source', true),
    new OptionItem('Sentiment', 'Sentiment', true),
    new OptionItem('Byline', 'Byline', true),
    new OptionItem('Contributor', 'Contributor', true),
    new OptionItem('Topic', 'Topic', true),
  ];
};
