import styled from 'styled-components';

export const SystemMessage = styled.div`
  .system-message-containing-box {
    padding: 1%;
  }

  .system-message-box {
    display: inline-block;
    position: relative;
    padding: 2%;
    height: 410px;
    float: left;
    background-color: ${(props) => props.theme.css.stickyNoteColor};
    @media (max-width: 768px) {
      margin-bottom: 0.5em;
      width: 92%;
    }
    @media (min-width: 1450px) {
      max-width: 50em;
      margin-left: 1em;
    }
  }
`;
