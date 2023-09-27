import * as styled from './styled';

export interface IAreaProps extends React.HTMLAttributes<HTMLDivElement> {
  children?: React.ReactNode;
}

/**
 * Custom div component used to contain the form for the Content View.
 * @returns Container area for the content form.
 */
export const Area: React.FC<any> = ({ children, ...rest }) => {
  return <styled.Area {...rest}>{children}</styled.Area>;
};
