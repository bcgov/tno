import * as styled from './styled';

export const Section: React.FC = ({ children, ...rest }) => {
  return <styled.Section {...rest}>{children}</styled.Section>;
};
