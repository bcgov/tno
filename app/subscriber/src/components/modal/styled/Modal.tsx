import styled from 'styled-components';
import { Col } from 'tno-core/dist/components/flex';

import { IModalProps } from '../Modal';

export const Modal = styled(Col)<IModalProps>`
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    z-index: 1040;
    width: 100vw;
    height: 100vh;
    background: ${(props) => props.theme.css.bkPrimary75};
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

  .modal {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    z-index: 100;
    background: white;
    position: relative;
    border-radius: 3px;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: fit-content;
    min-width: fit-content;
    max-width: 93%;
    background-color: rgba(255, 255, 255, 1);
    border: rgb(65, 57, 59) 1px solid;
    box-shadow: grey 5px 5px 5px;

    .modal-body {
      flex: 1;
      justify-content: center;
      padding: 1rem;
    }

    .button-row {
      margin-top: 1rem;
      gap: 1rem;
      justify-content: end;
      padding: 0.5rem;
    }
  }

  .modal-header {
    display: flex;
    background: rgb(86, 83, 122);
    border-top-right-radius: inherit;
    border-top-left-radius: inherit;
    color: white;
    padding: 0.5rem;
  }

  .modal-header span,
  .modal-header h1 {
    display: flex;
    align-items: center;
    margin: 0;
    font-size: 18px;
    font-weight: 500;
  }

  .modal-header h1 {
    flex: 1;
  }

  .modal-header span {
    padding: 0 10px;
  }
`;
