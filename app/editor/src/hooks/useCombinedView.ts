import { ContentTypeName } from 'hooks';
import React from 'react';

/**
 * If the URL path contains the path value '/combined/' then this page is a combined view.
 * @param contentType The default content type.
 * @returns True if the path is a combined view.
 */
export const useCombinedView = (contentType?: ContentTypeName) => {
  const pathname = window.location.pathname ?? '';
  const query = new URLSearchParams(window.location.search ?? '');
  const type = !query.get('form') ? contentType : (query.get('form') as ContentTypeName);

  const [combined, setCombined] = React.useState((pathname.match('/combined/')?.length ?? 0) > 0);
  const [formType, setFormType] = React.useState(type);

  React.useEffect(() => {
    setCombined((pathname.match('/combined/')?.length ?? 0) > 0);
  }, [pathname]);
  React.useEffect(() => {
    setFormType(type);
  }, [type]);

  return { combined, formType };
};
