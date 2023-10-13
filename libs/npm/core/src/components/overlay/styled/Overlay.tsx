import styled from 'styled-components';

import { IOverlayProps } from '../Overlay';

export const Overlay = styled.div<IOverlayProps>`
  position: absolute;
  top: 0;
  left: 0;
  background-color: rgba(0, 0, 0, 0.35);
  border-radius: 0.5rem;
  z-index: 2;
  width: 100%;
  height: 100%;
  cursor: not-allowed;
`;
