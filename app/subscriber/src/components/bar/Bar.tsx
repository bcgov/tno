import * as styled from './styled';

export interface IBarProps {
  /** Child components */
  children?: React.ReactNode;
}

/**
 * Provides a common styled horizontal bar.
 */
export const Bar: React.FC<IBarProps> = ({ children }) => {
  return <styled.Bar>{children}</styled.Bar>;
};
