import { navbarOptions } from 'components/navbar/NavbarItems';
import { useLocation } from 'react-router-dom';
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
  /** ignore min-width of fit-content for mobile */
  ignoreMinWidth?: boolean;
  /** Event fires when keyboard is pressed in section form */
  onKeyDown?: React.KeyboardEventHandler<HTMLDivElement>;
  /** Event fires when keyboard is pressed in section */
  onKeyDownCapture?: React.KeyboardEventHandler<HTMLDivElement>;
  includeHeaderIcon?: boolean;
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
  ignoreMinWidth,
  onKeyDown,
  onKeyDownCapture,
  includeHeaderIcon,
  ...rest
}) => {
  const pathname = useLocation()?.pathname.replace(/^\/|\/$/g, '');
  const icon = navbarOptions.find((item) => item.path.includes(pathname ?? ''))?.icon;
  return (
    <styled.PageSection
      $ignoreLastChildGap={ignoreLastChildGap}
      $ignoreMinWidth={ignoreMinWidth}
      className={`page-section${className ? ` ${className}` : ''}`}
      onKeyDown={onKeyDown}
      onKeyDownCapture={onKeyDownCapture}
      {...rest}
    >
      {header && (
        <Row className="page-section-title">
          {includeHeaderIcon && icon && <div className="page-icon">{icon}</div>}
          {header}
        </Row>
      )}
      {children}
    </styled.PageSection>
  );
};
