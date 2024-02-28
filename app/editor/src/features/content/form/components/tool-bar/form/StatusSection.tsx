import { Status } from 'components/status';
import { ContentNavigation } from 'features/content/form';
import { IContentForm } from 'features/content/form/interfaces';
import { getStatusText } from 'features/content/list-view/utils';
import { Col, Row, Show, ToolBarSection } from 'tno-core';

export interface IStatusSectionProps {
  /** Form values. */
  values: IContentForm;
  /** Function to fetch content. */
  fetchContent: (id: number) => void;
  /** Root path for combined view. */
  combinedPath?: string;
}

/**
 * A component that displays the content status.
 * @param param0 Component properties.
 * @returns Component.
 */
export const StatusSection: React.FC<IStatusSectionProps> = ({
  values,
  fetchContent,
  combinedPath,
}) => {
  return (
    <Col alignContent="center" className="toolbar-status">
      <ToolBarSection>
        <Col>
          <ContentNavigation values={values} fetchContent={fetchContent} />
          <Row justifyContent="center" className="title-container">
            Details
          </Row>
          <Row justifyContent="center" className="white-bg" gap="0.5rem">
            {getStatusText(values.status)}
            <Status value={values.status} />
          </Row>
          <Show visible={values.id !== 0}>
            <Row justifyContent="center" className="white-bg">
              <span>ID: {values.id}</span>
            </Row>
          </Show>
        </Col>
      </ToolBarSection>
    </Col>
  );
};
