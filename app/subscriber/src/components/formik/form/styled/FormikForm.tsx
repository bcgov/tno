import styled from 'styled-components';

export const FormikForm = styled.div`
  position: relative;
  padding: 0.25em;
  border-radius: 0.25em;

  form {
    background-color: ${(props) => props.theme.css.formBackgroundColor};
  }
`;
