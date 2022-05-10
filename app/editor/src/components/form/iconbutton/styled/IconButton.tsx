import styled from 'styled-components';
import { Button } from 'tno-core';

export const IconButton = styled(Button)<any>`
  img {
    margin-right: ${(props) => !!props.label && '0.5em'};
  }
`;
