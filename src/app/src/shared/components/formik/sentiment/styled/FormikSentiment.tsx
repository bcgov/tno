import styled from 'styled-components';

import { IFormikSentimentProps } from '../FormikSentiment';

export const FormikSentiment = styled.div.attrs<IFormikSentimentProps<any>>(({ required }) => ({
  required,
}))<IFormikSentimentProps<any>>`
  display: flex;
  flex-direction: column;

  img {
    height: 20px;
    width: 20px;
    align-self: center;
  }
  button {
    background-color: #f2f2f2;
    border-radius: 15%;
    border-color: #f2f2f2;
    margin: 0 0.25rem;
    margin-top: 0.5rem;
    box-shadow: 0px 1px 2px rgba(0, 0, 0, 0.2);
    min-width: 2rem;
    min-height: 2rem;
    align-self: center;
    &:hover {
      cursor: pointer;
    }
  }

  .blank {
    height: 20px;
    width: 20px;
  }

  .active {
    background-color: #007af5;
    color: white;
    // box-shadow: none;
    border: none;
    min-height: 2rem;
    min-width: 2rem;
  }

  label {
    align-self: flex-start;
    font-weight: 600;
    :after {
      content: '${(props) => (props.required ? ' *' : '')}';
      color: red;
    }
  }
`;
