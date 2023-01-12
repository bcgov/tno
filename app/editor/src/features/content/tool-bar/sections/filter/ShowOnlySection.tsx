import { ToolBarSection } from 'components/tool-bar';
import { IContentListFilter } from 'features/content/list-view/interfaces';
import { ContentTypeName } from 'hooks';
import { FaEye } from 'react-icons/fa';
import { useContent } from 'store/hooks';
import { Checkbox, Col, Row } from 'tno-core';

export interface IShowOnlySectionProps {
  onChange: (filter: IContentListFilter) => void;
}

/**
 * Show only section
 * @param onChange determines what happens when the checkbox is checked or unchecked
 * @returns A section with checkboxes to show only certain content types
 */
export const ShowOnlySection: React.FC<IShowOnlySectionProps> = ({ onChange }) => {
  const [{ filter }] = useContent();

  return (
    <ToolBarSection
      children={
        <Col>
          <Row>
            <Checkbox
              name="isPrintContent"
              label="Print Content"
              tooltip="Newspaper content without audio/video"
              checked={filter.contentType === ContentTypeName.PrintContent}
              onChange={(e) => {
                onChange({
                  ...filter,
                  pageIndex: 0,
                  contentType: e.target.checked ? ContentTypeName.PrintContent : undefined,
                });
              }}
            />
            <Checkbox
              name="includedInCategory"
              label="Included in EoD"
              tooltip="Content included in Event of the Day"
              value={filter.includedInCategory}
              checked={filter.includedInCategory}
              onChange={(e) => {
                onChange({
                  ...filter,
                  pageIndex: 0,
                  includedInCategory: e.target.checked,
                });
              }}
            />
            <Checkbox
              name="ticker"
              label="On Ticker"
              value="On Ticker"
              tooltip="Content identified as on ticker"
              checked={filter.onTicker !== ''}
              onChange={(e) => {
                onChange({
                  ...filter,
                  pageIndex: 0,
                  onTicker: e.target.checked ? e.target.value : '',
                });
              }}
            />
          </Row>
          <Row>
            <Checkbox
              name="commentary"
              label="Commentary"
              value="Commentary"
              tooltip="Content identified as commentary"
              checked={filter.commentary !== ''}
              onChange={(e) => {
                onChange({
                  ...filter,
                  pageIndex: 0,
                  commentary: e.target.checked ? e.target.value : '',
                });
              }}
            />
            <Checkbox
              name="topStory"
              label="Top Story"
              value="Top Story"
              tooltip="Content identified as a top story"
              checked={filter.topStory !== ''}
              onChange={(e) => {
                onChange({
                  ...filter,
                  pageIndex: 0,
                  topStory: e.target.checked ? e.target.value : '',
                });
              }}
            />
          </Row>
        </Col>
      }
      label="SHOW ONLY"
      icon={<FaEye />}
    />
  );
};
