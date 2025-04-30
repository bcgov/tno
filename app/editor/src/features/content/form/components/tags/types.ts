import { IContentForm } from 'features/content/form/interfaces';
import { IOptionItem } from 'tno-core';

/**
 * Interface for content tag structure
 */
export interface IContentTag {
  id: number;
  name: string;
}

// Re-export IContentForm for convenience
export type { IContentForm };

/**
 * Interface for tag data structure
 */
export interface Tag {
  id: number;
  code: string;
  name: string;
  description?: string;
  sortOrder: number;
  isEnabled: boolean;
}

/**
 * Interface for parsed tag with original format
 */
export interface IParsedTag {
  tag: string;
  original: string;
}

/**
 * Interface for the tags context state
 */
export interface ITagsContextState {
  /** Dropdown menu tag options */
  tagOptions: IOptionItem[];
  /** Selected options in dropdown menu */
  selectedOptions: IOptionItem[];
  /** Show tag list */
  showList: boolean;
  /** Set show tag list */
  setShowList: (show: boolean) => void;
  /** Handle tag selection changes */
  addTags: (selectedTagOptions: IOptionItem[]) => void;
}

/**
 * Interface for tags provider props
 */
export interface ITagsProviderProps {
  children: React.ReactNode;
  /** Default tags to include */
  defaultTags?: string[];
  /** The field to update with tags */
  targetField?: 'body' | 'summary';
  /** Whether to enable automatic tag text updates */
  enableAutoTagText?: boolean;
  /** Allow external injection of these values (mainly for testing) */
  values?: IContentForm;
  setFieldValue?: (field: string, value: any) => void;
  tags?: Tag[];
}

/**
 * Interface for Tags component props
 */
export interface ITagsProps {
  defaultTags?: string[];
  /** the field to update with tags */
  targetField?: 'body' | 'summary';
  /** whether to enable automatic tag text updates, default is true */
  enableAutoTagText?: boolean;
}
