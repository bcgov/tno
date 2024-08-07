import styled from 'styled-components';

export const SystemMessage = styled.div`
  .system-message-box {
    border: 1px solid ${(props) => props.theme.css.lineTertiaryColor};
  }
  width: 80%;
  padding: 0.5em;
  margin-left: auto;
  margin-right: auto;
  margin-top: 1.5em;
  border-radius: 0.5em;
  .bell-icon {
    float: right;
    color: ${(props) => props.theme.css.btnBkPrimary};
  }
`;
