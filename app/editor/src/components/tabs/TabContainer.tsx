import { Col, Row } from 'components';

import * as styled from './TabContainerStyled';

export interface ITabContainerProps {
  borderColour?: string;
  height?: string;
  width?: string;
  children?: React.ReactNode;
  /**
   * provide a row of tabs to display at the top of the tab component
   */
  tabs?: React.ReactNode;
}

export const TabContainer: React.FC<ITabContainerProps> = ({ children, tabs }) => {
  return (
    <>
      <Col>
        <Row>{tabs}</Row>
        <styled.TabContainer>{children}</styled.TabContainer>
      </Col>
    </>
  );
};
