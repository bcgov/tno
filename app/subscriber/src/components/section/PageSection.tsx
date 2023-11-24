import { Row } from 'tno-core';

import * as styled from './styled';

export interface IPageSectionProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Header components or text to identify this page section. */
  header?: React.ReactNode;
  /** Child components */
  children?: React.ReactNode;
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
  ...rest
}) => {
  return (
    <styled.PageSection className={`page-section${className ? ` ${className}` : ''}`} {...rest}>
      {header && <Row className="page-section-title">{header}</Row>}
      {children}
    </styled.PageSection>
  );
};
