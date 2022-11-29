import { Row } from 'tno-core';

import * as styled from './styled';

export interface IToolBarSectionProps {
  actions: React.ReactNode;
  label: string;
  icon: React.ReactNode;
}
/**
 * Creates a section for the parent ToolBar component
 * @param actions Provide buttons/ filter actions for given section
 * @param label Provide a label for the section
 * @param icon Provide an icon for the section
 * @returns A ToolBar section
 */
export const ToolBarSection: React.FC<IToolBarSectionProps> = ({ actions, label, icon }) => {
  return (
    <styled.ToolBarSection className="section">
      <Row className="children-container">{actions}</Row>
      <Row className="label-container">
        {icon}
        {label}
      </Row>
    </styled.ToolBarSection>
  );
};
