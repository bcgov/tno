import { Dialog as HDialog } from '@headlessui/react';
import styled from 'styled-components';

export const Dialog = styled<typeof HDialog>(HDialog)`
  position: absolute;
  top: 0;
`;

export default Dialog;
