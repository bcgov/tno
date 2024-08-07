import styled from 'styled-components';

export const SystemMessage = styled.div`
  .system-message-box {
    border: 1px solid ${(props) => props.theme.css.btnBkPrimary};
    margin-right: 1em;
    margin-left: 1em;
    border-radius: 0.5em;
    padding: 0.5em;
  }
  margin-top: 1.5em;
  .bell-icon {
    float: right;
    color: ${(props) => props.theme.css.btnBkPrimary};
  }
`;
