import { Row } from '../flex';
import { Show } from '../show';
import * as styled from './styled';

export interface IToolBarSectionProps extends React.HtmlHTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
  label?: string;
  icon?: React.ReactNode;
  title?: string;
}
/**
 * Creates a section for the parent ToolBar component
 * @param children Provide buttons/ filter actions for given section
 * @param label Provide a label for the section
 * @param icon Provide an icon for the section
 * @returns A ToolBar section
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
