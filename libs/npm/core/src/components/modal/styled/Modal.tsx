import styled from 'styled-components';

import { Col } from '../../flex';
import { IModalProps } from '../Modal';

export const Modal = styled(Col)<IModalProps>`
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    z-index: 1040;
    width: 100vw;
    height: 100vh;
    background: rgba(0, 0, 0, 0.9);
  }

  .modal-wrapper {
    position: fixed;
    top: 0;
    left: 0;
    z-index: 1050;
    width: 100%;
    height: 100%;
    overflow-x: hidden;
    overflow-y: auto;
    outline: 0;
  }

  .button-row {
    margin-top: 0.2rem;
    button {
      margin-right: 2%;
    }
  }

  .modal-popup {
    display: flex;
    flex-direction: column;
    align-items: stretch;
    z-index: 100;
    background: white;
    position: absolute;
    left: 50%;
    top: 50%;
    margin: 2%;
    border-radius: 3px;
    min-width: fit-content;
    max-width: 93%;
    padding: 1.5rem;
    background-color: rgba(255, 255, 255, 1);
    height: ${(props) => props.hasHeight && '-webkit-fill-available'};
    width: -webkit-fill-available;
    transform: translate(-50%, -50%);
  }

  .modal-full {
    display: flex;
    flex-direction: column;
    align-items: stretch;
    z-index: 100;
    background: white;
    margin: 2%;
    border-radius: 3px;
    min-width: fit-content;
    max-width: 93%;
    padding: 1.5rem;
    background-color: rgba(255, 255, 255, 1);
    height: ${(props) => props.hasHeight && '-webkit-fill-available'};
    width: -webkit-fill-available;
  }

  .modal-header {
    display: flex;
  }

  .modal-body {
    flex: 1;
  }
`;
