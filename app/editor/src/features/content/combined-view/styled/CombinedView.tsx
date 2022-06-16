import { FormPage } from 'components/form/formpage/styled';
import styled from 'styled-components';

export const CombinedView = styled(FormPage)`
  min-height: calc(100% - 500px);
  min-width: 95%;
  overflow: hidden;
  .reflex-element {
    padding-top: 0.5em;
    height: 65%;
  }
  .filter-area {
    background-color: #f5f5f5;
  }
  .content-filter {
    max-width: fit-content;
    margin-left: auto;
    margin-right: auto;

    .box {
      margin-top: 0.6em;
      margin-left: 1em;
      border: solid 1px grey;
      border-radius: 0.25em;
      padding: 1em;
      max-width: fit-content;
    }
  }
`;
