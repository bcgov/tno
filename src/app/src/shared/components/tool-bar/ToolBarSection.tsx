import { Row } from '../flex';
import { Show } from '../show';
import * as styled from './styled';

export interface IToolBarSectionProps extends React.HtmlHTMLAttributes<HTMLDivElement> {
  /** Title of the section. */
  title?: string;
  /** Label at bottom of section. */
  label?: string;
  /** Label icon at bottom of section. */
  icon?: React.ReactNode;
}
/**
 * Creates a section for the parent ToolBar component.
 * @param props Component properties.
 * @returns A ToolBar section.
 */
export const ToolBarSection: React.FC<IToolBarSectionProps> = ({
  children,
  label,
  icon,
  title,
  className,
  ...rest
}) => {
  return (
    <styled.ToolBarSection className={`tool-bar${className ? ` ${className}` : ''}`} {...rest}>
      <Show visible={!!title}>
        <Row className="title-container">{title}</Row>
      </Show>
      <Row className={!!label ? 'children-container' : 'title-children'}>{children}</Row>
      <Row className="label-container">
        {icon}
        {label}
      </Row>
    </styled.ToolBarSection>
  );
};
