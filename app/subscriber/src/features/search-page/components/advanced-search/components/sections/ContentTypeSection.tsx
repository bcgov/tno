import { useContent } from 'store/hooks';
import { Button, ButtonVariant, Checkbox, ContentTypeName, Row, Select, Show } from 'tno-core';

export interface IContentTypeSectionProps {
  /** whether to display filter choices as checkboxes or a select box */
  displayFiltersAsDropdown: boolean;
}

/** section that allows you to filter down based on content type with a drop down menu */
export const ContentTypeSection: React.FC<IContentTypeSectionProps> = ({
  displayFiltersAsDropdown,
}) => {
  const [
    {
      search: { filter },
    },
    { storeSearchFilter: storeFilter },
  ] = useContent();

  const typeOptions = [
    { label: 'Audio/Video', value: ContentTypeName.AudioVideo },
    { label: 'Print Content', value: ContentTypeName.PrintContent },
    { label: 'Image', value: ContentTypeName.Image },
    { label: 'Internet', value: ContentTypeName.Internet },
  ];

  return (
    <Row className="content-types-container">
      <Show visible={!displayFiltersAsDropdown}>
        <div className="check-box-list">
          <div className="chk-box-container chk-content-type select-all">
            <Button
              variant={ButtonVariant.link}
              onClick={() =>
                storeFilter({ ...filter, contentTypes: typeOptions.map((m) => m.value!) })
              }
            >
              Select All
            </Button>
            /
            <Button
              variant={ButtonVariant.link}
              onClick={() => storeFilter({ ...filter, contentTypes: [] })}
            >
              Deselect All
            </Button>
          </div>
          {typeOptions.map((item, index) => (
            <div key={index} className="chk-box-container chk-content-type">
              <Checkbox
                id={`chk-content-type-${index}`}
                key={index}
                label={item.label}
                checked={filter.contentTypes?.includes(item.value)}
                value={item.value}
                onChange={(e) => {
                  storeFilter({
                    ...filter,
                    contentTypes: e.target.checked
                      ? [...filter.contentTypes!, e.target.value as ContentTypeName] // add it
                      : filter.contentTypes?.filter((i) => i !== e.target.value), // remove it
                  });
                }}
              />
            </div>
          ))}
        </div>
      </Show>
      <Show visible={displayFiltersAsDropdown}>
        <Select
          name="content-types"
          isMulti
          key={filter.contentTypes?.join(',')}
          className="content-types centered"
          width="25em"
          options={typeOptions}
          onChange={(newValues) => {
            Array.isArray(newValues) &&
              storeFilter({
                ...filter,
                contentTypes: newValues.map((v) => v.value),
              });
          }}
          value={typeOptions.filter((o) => o.value && filter.contentTypes?.includes(o.value))}
        />
      </Show>
    </Row>
  );
};
