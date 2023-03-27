import React from 'react';
import { Col, IToolBarSectionProps, Row, Show, ToolBarSection } from 'tno-core';

export interface ITitleSectionProps extends IToolBarSectionProps {
  /** Section icon. */
  picture?: React.ReactElement;
}

/**
 * Provides a component tool-bar section that displays a title and icons.
 * @param param0 Component properties.
 * @returns Component.
 */
export const TitleSection: React.FC<ITitleSectionProps> = ({ title, picture, ...rest }) => {
  return (
    <ToolBarSection {...rest}>
      <Col>
        <Show visible={!!title}>
          <Row className="title-container">{title}</Row>
        </Show>
        {picture &&
          React.cloneElement(picture, {
            className: `title-icon${picture.props.className ? ` ${picture.props.className}` : ''}`,
          })}
      </Col>
    </ToolBarSection>
  );
};
