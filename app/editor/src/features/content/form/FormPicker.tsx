import { ContentType } from 'hooks/api-editor';
import React from 'react';

import { ContentForm } from './ContentForm';

export interface IFormPickerProps {
  /** The content type this form will create */
  contentType?: ContentType;
}

/**
 * Based on the specified 'contentType' it will load the appropriate form component.
 * @returns Component to view/edit content.
 */
export const FormPicker: React.FC<IFormPickerProps> = ({ contentType = ContentType.Snippet }) => {
  switch (contentType) {
    case ContentType.Print:
    case ContentType.Radio:
    case ContentType.TV:
    case ContentType.Frontpage:
    case ContentType.Snippet:
    default:
      return <ContentForm contentType={contentType} />;
  }
};
