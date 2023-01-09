import * as styled from './styled';

export interface IToolBarProps extends React.HTMLAttributes<HTMLDivElement> {
  variant?: 'dark' | 'light';
}

/**
 * The parent ToolBar container
 * @param children Provide the sections for the ToolBar
 * @returns The ToolBar container
 */
export const ToolBar: React.FC<IToolBarProps> = ({ children, variant }) => {
  return <styled.ToolBar variant={variant}>{children}</styled.ToolBar>;
};
