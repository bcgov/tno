import styled from 'styled-components';

export const Text = styled.div`
  display: flex;
  flex-direction: column;
  padding-right: 0.5em;

  p[role='alert'] {
    font-size: 0.85em;
    color: ${(props) => props.theme.css.dangerColor};
  }

  input[role='alert'] {
    border-color: ${(props) => props.theme.css.dangerColor};
    filter: grayscale(100%) brightness(65%) sepia(25%) hue-rotate(-50deg) saturate(600%)
      contrast(0.8);
  }

  & > label {
    white-space: nowrap;
  }

  input[disabled] {
    color: hsl(0, 0%, 50%);
    background-color: hsl(0, 0%, 95%);
  }

  .required:after {
    content: ' *';
    color: ${(props) => props.theme.css.dangerColor};
  }
`;
