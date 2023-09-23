import { ContentActions } from 'features/content/form';
import { IContentForm } from 'features/content/form/interfaces';
import React from 'react';
import { FaHighlighter } from 'react-icons/fa';
import {
  ActionName,
  ContentTypeName,
  IActionModel,
  Row,
  ToolBarSection,
  ValueType,
} from 'tno-core';

export interface IActionSectionProps {
  /** Form values. */
  values: IContentForm;
}

/**
 * Component to display actions for the specified content.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ActionSection: React.FC<IActionSectionProps> = ({ values }) => {
  const determineActions = React.useCallback((contentType: ContentTypeName) => {
    switch (contentType) {
      case ContentTypeName.PrintContent:
        return (a: IActionModel) =>
          a.valueType === ValueType.Boolean && a.name !== ActionName.NonQualified;
      case ContentTypeName.Image:
        return (a: IActionModel) => a.name === ActionName.Homepage;
      case ContentTypeName.AudioVideo:
        return (a: IActionModel) => {
          return a.valueType === ValueType.Boolean && a.name !== ActionName.TopStory;
        };
      default:
        return (a: IActionModel) =>
          a.valueType === ValueType.Boolean && a.name !== ActionName.Alert;
    }
  }, []);

  return (
    <ToolBarSection
      label="HIGHLIGHT STORY"
      icon={<FaHighlighter />}
      children={
        <Row>
          <ContentActions
            init
            filter={determineActions(values.contentType)}
            contentType={values.contentType}
          />
          <ContentActions filter={(a) => a.valueType !== ValueType.Boolean} />
        </Row>
      }
    />
  );
};
