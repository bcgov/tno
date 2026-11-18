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
export const Bar: React.FC<IBarProps> = ({ children, vanilla = false, className, ...rest }) => {
  return (
    <styled.Bar className={`bar${className ? ` ${className}` : ''}`} {...rest} $vanilla={vanilla}>
      {children}
    </styled.Bar>
  );
};
