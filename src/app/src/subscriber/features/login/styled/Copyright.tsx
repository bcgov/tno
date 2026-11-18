import styled from 'styled-components';

export const Copyright = styled.div`
  padding: 2%;
  text-align: left;
  border-radius: 0 0 0.5em 0.5em;
  margin-top: 2%;
  bottom: 0;
  left: 0;
  background-color: ${(props) => props.theme.css.dialogBoxBkSecondary};
`;
