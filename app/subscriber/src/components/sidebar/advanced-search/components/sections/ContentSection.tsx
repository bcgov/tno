import { useContent } from 'store/hooks';
import { ContentTypeName, Row, Select } from 'tno-core';

/** section that allows you to filter down based on content type with a drop down menu */
export const ContentSection: React.FC = () => {
  const [{ searchFilter: filter }, { storeSearchFilter: storeFilter }] = useContent();
  const typeOptions = [
    { label: 'Audio/Video', value: ContentTypeName.AudioVideo },
    { label: 'Print Content', value: ContentTypeName.PrintContent },
    { label: 'Image', value: ContentTypeName.Image },
    { label: 'Story', value: ContentTypeName.Story },
  ];
  return (
    <Row className="content-types-container" justifyContent="center">
      <Select
        name="content-types"
        isMulti
        key={filter.contentTypes?.join(',')}
        className="content-types centered"
        width="25em"
        defaultValue={typeOptions.filter((t) => {
          return filter.contentTypes?.includes(t.value);
        })}
        options={typeOptions}
        onChange={(newValues) => {
          Array.isArray(newValues) &&
            storeFilter({
              ...filter,
              contentTypes: newValues.map((v) => v.value),
            });
        }}
      />
    </Row>
  );
};
