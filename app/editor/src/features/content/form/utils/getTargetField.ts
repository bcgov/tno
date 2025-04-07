import { ContentTypeName } from 'tno-core';

/**
 * Determine the target field based on content type and the current active tab
 * @param contentType - The content type
 * @param activeTab - The current active tab
 * @returns 'body' or 'summary' as the target field
 */
export const getTargetField = (contentType: ContentTypeName, activeTab: string): 'body' | 'summary' => {
  // If the content type is PrintContent or Internet, use the body field
  if (contentType === ContentTypeName.PrintContent || contentType === ContentTypeName.Internet) {
    return 'body';
  }
  
  // If the current tab is transcript, use the body field
  if (activeTab === 'transcript') {
    return 'body';
  }
  
  // In other cases, use the summary field
  return 'summary';
};