import { IRowProps } from './../flex';
import * as styled from './styled';

export interface IToolBarProps extends IRowProps {
  variant?: 'dark' | 'light';
}

/**
 * The parent ToolBar container
 * @param children Provide the sections for the ToolBar
 * @returns The ToolBar container
 */
export const ToolBar: React.FC<IToolBarProps> = ({ children, variant, ...rest }) => {
  return (
    <styled.ToolBar variant={variant} {...rest}>
      {children}
    </styled.ToolBar>
  );
};
