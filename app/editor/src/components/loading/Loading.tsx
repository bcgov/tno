import { Spinner } from 'components';

import * as styled from './LoadingStyled';

/**
 * Loading provides an overlay with a spinner to indicate to the user something is loading.
 * @param props Div element attributes.
 * @returns Loading component.
 */
export const Loading: React.FC = (props) => {
  return (
    <styled.Loading>
      <Spinner size="4rem" />
      {props.children}
    </styled.Loading>
  );
};
