import styled from 'styled-components';

export const FormikText = styled.div`
  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
  }
`;
