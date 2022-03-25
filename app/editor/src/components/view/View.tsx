import * as styled from './styled';

export const View: React.FC = ({ children, ...rest }) => {
  return <styled.View {...rest}>{children}</styled.View>;
};
