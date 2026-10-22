import React from 'react';

import { TextArea, Wysiwyg } from '../form';

/** The editor a filter prompt is written in. */
export type FilterAIPromptEditorType = 'wysiwyg' | 'textarea';

export interface IFilterAIPromptEditorProps {
  name: string;
  label?: string;
  placeholder?: string;
  value: string;
  editor: FilterAIPromptEditorType;
  onChange: (value: string) => void;
}

/**
 * The prompt editor: a rich text editor, or a plain text area when formatting is not needed.
 */
export const FilterAIPromptEditor: React.FC<IFilterAIPromptEditorProps> = ({
  name,
  label,
  placeholder,
  value,
  editor,
  onChange,
}) =>
  editor === 'textarea' ? (
    <TextArea
      name={name}
      label={label || undefined}
      placeholder={placeholder}
      value={value}
      rows={5}
      onChange={(e) => onChange(e.target.value)}
    />
  ) : (
    <Wysiwyg
      name={name}
      label={label}
      placeholder={placeholder}
      value={value}
      onChange={(html: string) => onChange(html)}
    />
  );
