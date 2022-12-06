import { Row } from 'tno-core';
import * as styled from './styled';

export interface IToolBarSection2Props {
  label: string;
}
/**
 * Creates a section for the parent ToolBar component
 * @param actions Provide buttons/ filter actions for given section
 * @param label Provide a label for the section
 * @returns A ToolBar section
 */
export const ToolBarSection2: React.FC<IToolBarSection2Props> = ({ label }) => {
  return (
    <styled.ToolBarSection className="details-section">
      <Row className="title-container">Content Details</Row>
      <Row className="status-container">{label}</Row>
    </styled.ToolBarSection>
  );
};

