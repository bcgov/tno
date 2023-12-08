import * as styled from './styled';

export interface IBarProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Child components */
  children?: React.ReactNode;
  /** Plain, barebones variant */
  vanilla?: boolean;
}

/**
 * Provides a common styled horizontal bar.
 */
export const Bar: React.FC<IBarProps> = ({ children, vanilla = false, ...rest }) => {
  return (
    <styled.Bar {...rest} $vanilla={vanilla}>
      {children}
    </styled.Bar>
  );
};
