import { ContentType } from 'hooks/api-editor';
import React from 'react';

import { ContentForm } from './ContentForm';

export interface IFormPickerProps {
  /** The content type this form will create */
  contentType: ContentType;
}

/**
 * Based on the specified 'contentType' it will load the appropriate form component.
 * @returns Component to view/edit content.
 */
export const FormPicker: React.FC<IFormPickerProps> = ({ contentType }) => {
  switch (contentType) {
    case ContentType.Print:
      return <ContentForm contentType={ContentType.Print} />;
    case ContentType.Radio:
    case ContentType.TV:
    case ContentType.Frontpage:
    case ContentType.Snippet:
      return <ContentForm contentType={ContentType.Snippet} />;
    default:
      // default to snippet until other forms are created
      return <ContentForm contentType={ContentType.Snippet} />;
  }
};
