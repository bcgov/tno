import { Button, Dropdown, Text } from 'components';

export const ListView: React.FC = () => {
  return (
    <div>
      <Dropdown
        name="mediaType"
        label="Media Type"
        options={['All Media', 'Syndication']}
      ></Dropdown>
      <Button>Test</Button>
      <Text />
    </div>
  );
};
