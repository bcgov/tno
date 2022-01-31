import styled from 'styled-components';

export const FormikTextArea = styled.div`
  background-color: ${(props) => props.theme.css.formBackgroundColor};

  [role='alert'] {
    font-weight: 0.85em;
    color: ${(props) => props.theme.css.dangerColor};
  }

  textarea.error {
    border-color: ${(props) => props.theme.css.dangerColor};
    filter: grayscale(100%) brightness(65%) sepia(25%) hue-rotate(-50deg) saturate(600%)
      contrast(0.8);
  }
`;
