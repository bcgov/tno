import styled from 'styled-components';

export const Modal = styled.div`
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
    margin-top: 10%;
    button {
      margin-right: 2%;
    }
  }

  .modal {
    z-index: 100;
    background: white;
    position: relative;
    margin: 10% auto;
    border-radius: 3px;
    max-width: 600px;
    padding: 2rem;
    background-color: rgba(255, 255, 255, 1);
  }

  .modal-header {
    display: flex;
  }
`;
