import { useFormikContext } from 'formik';
import { Col, FormikCheckbox, FormikText, IQuoteModel, Row } from 'tno-core';

import { IContentForm } from './interfaces';
import * as styled from './styled';

/**
 * Provides a way to view/edit content quotes.
 * @returns the ContentQuotesForm
 */
export const ContentQuotesForm: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();

  const handleBylineChange = (quote: IQuoteModel, byline: string) => {
    var results = [...values.quotes];
    results.find((q) => q.id === quote.id)!.byline = byline;
    setFieldValue('quotes', results);
  };

  const handleStatementChange = (quote: IQuoteModel, statement: string) => {
    var results = [...values.quotes];
    results.find((q) => q.id === quote.id)!.statement = statement;
    setFieldValue('quotes', results);
  };

  const handleIsRelevantChange = (quote: IQuoteModel, isRelevant: boolean) => {
    var results = [...values.quotes];
    results.find((q) => q.id === quote.id)!.isRelevant = isRelevant;
    setFieldValue('quotes', results);
  };

  const quotes = values.quotes.map((row, index) => {
    return (
      <Row key={`${index}-${row.id}`}>
        <Col flex="0.5 0.5 0">
          <FormikText
            name={`quotes.${index}.byline`}
            onChange={(e) => {
              handleBylineChange(row, e.target.value);
            }}
          />
        </Col>
        <Col flex="2 1.5 0">
          <FormikText
            name={`quotes.${index}.statement`}
            onChange={(e) => {
              handleStatementChange(row, e.target.value);
            }}
          />
        </Col>
        <Col flex="0 0 auto">
          <FormikCheckbox
            name={`quotes.${index}.isRelevant`}
            checked={values.quotes[index].isRelevant}
            tooltip="Hidden quotes will NOT be displayed on the Subscriber site"
            onChange={(e) => {
              handleIsRelevantChange(row, e.target.checked);
            }}
          />
        </Col>
      </Row>
    );
  });
  return (
    <styled.ContentQuotesForm>
      <div className="quotes-container">
        <Row className="header">
          <Col flex="0.5 0.5 0">Byline</Col>
          <Col flex="2 1.5 0">Statement</Col>
          <Col flex="0 0 auto">Show</Col>
        </Row>
        {quotes}
      </div>
    </styled.ContentQuotesForm>
  );
};
