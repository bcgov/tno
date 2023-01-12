import styled from 'styled-components';

import { Button } from '../../..';
import { IIconButtonProps } from '../IconButton';

export const IconButton = styled(Button)<IIconButtonProps>`
  img {
    margin-right: ${(props) => !!props.label && '0.5em'};
  }
`;
