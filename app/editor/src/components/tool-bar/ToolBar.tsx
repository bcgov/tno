import * as styled from './styled';

/**
 * The parent ToolBar container
 * @param children Provide the sections for the ToolBar
 * @returns The ToolBar container
 */
export const ToolBar: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({ children }) => {
  return <styled.ToolBar>{children}</styled.ToolBar>;
};
