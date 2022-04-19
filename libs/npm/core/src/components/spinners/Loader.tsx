import { ISpinnerProps, Spinner } from '.';
import * as styled from './styled';

export interface ILoaderProps extends ISpinnerProps {
  visible?: boolean;
}

export const Loader: React.FC<ILoaderProps> = ({ visible, ...rest }) => {
  return visible ? (
    <styled.Loader>
      <Spinner {...rest} />
    </styled.Loader>
  ) : null;
};
