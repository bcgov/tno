import styled from 'styled-components';

export const AccessRequest = styled.div`
  display: flex;
  flex-direction: column;
  width: 100%;
  margin: 0 5% 0 5%;
  padding: 2em;
  background-color: ${(props) => props.theme.css.formBackgroundColor};

  button {
    margin-bottom: 0.5em;
  }

  & > div[direction='row'] {
    margin-top: 2em;
  }

  .preApproval {
    background-color: #f5f5f5;
    border-radius: 1em;
    padding: 1em 2em 1em 2em;
    box-shadow: 4px 3px 8px 1px #969696;

    form {
      background-color: transparent;
    }
  }

  .register {
    background-color: #f5f5f5;
    border-radius: 1em;
    padding: 1em 2em 1em 2em;
    box-shadow: 4px 3px 8px 1px #969696;

    form {
      background-color: transparent;

      div {
        button {
          margin-left: auto;
        }
      }
    }
  }
`;
