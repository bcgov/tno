import { ContentActionBar } from 'components/tool-bar';
import { IContentModel, Row } from 'tno-core';

import * as styled from './styled';

export interface IPageSectionProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Header components or text to identify this page section. */
  header?: React.ReactNode;
  /** Child components */
  children?: React.ReactNode;
  /** ignore last child gap */
  ignoreLastChildGap?: boolean;
  /** include content action bar */
  includeContentActions?: boolean;
  /** context of content if needed */
  activeContent?: IContentModel[];
  /** set active content */
  setActiveContent?: (content: IContentModel[]) => void;
  /** ignore min-width of fit-content for mobile */
  ignoreMinWidth?: boolean;
}

/**
 * Provides a common page section styled component.
 * @param param0 Component properties
 * @returns Component
 */
export const PageSection: React.FC<IPageSectionProps> = ({
  header,
  className,
  children,
  ignoreLastChildGap,
  includeContentActions,
  activeContent,
  setActiveContent,
  ignoreMinWidth,
  ...rest
}) => {
  return (
    <styled.PageSection
      $ignoreLastChildGap={ignoreLastChildGap}
      $ignoreMinWidth={ignoreMinWidth}
      className={`page-section${className ? ` ${className}` : ''}`}
      {...rest}
    >
      {includeContentActions && !!activeContent && (
        <ContentActionBar className="content-actions" content={activeContent} />
      )}
      {header && <Row className="page-section-title">{header}</Row>}
      {children}
    </styled.PageSection>
  );
};
