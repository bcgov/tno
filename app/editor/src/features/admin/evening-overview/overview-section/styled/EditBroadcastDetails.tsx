import styled from 'styled-components';
import { Col } from 'tno-core';

export const EditBroadcastDetails = styled(Col)`
  transition: all 0.3s ease;
  /* add border */
  border: 1px solid #ccc;
  box-shadow: 0 0 5px 0 rgba(0, 0, 0, 0.2);
  &.in {
    animation: fade-in 0.5s linear;
  }

  &.out {
    animation: fade-out 0.5s linear;
  }

  margin-bottom: 1em;
  .edit-header {
    background-color: rgb(245, 245, 245);
    padding: 0.5em;
  }
  .edit-contents {
    padding: 0.5em;
  }
  .tools {
    margin-top: 1em;
    .buttons {
      margin-left: auto;
      .delete {
        align-self: center;
        margin-left: 0.5em;
      }
    }
  }

  @keyframes fade-in {
    from {
      opacity: 0;
      visibility: hidden;
      transform: translateY(-10px);
    }
    to {
      opacity: 1;
      visibility: visible;
      transform: translateY(0px);
    }
  }

  @keyframes fade-out {
    from {
      opacity: 1;
      visibility: visible;
      transform: translateY(0px);
    }
    to {
      opacity: 0;
      visibility: hidden;
      height: 0;
      transform: translateY(-10px);
    }
  }
`;
