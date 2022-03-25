import { Button, ButtonVariant } from 'components/button';
import { Col } from 'components/flex/col';
import { Row } from 'components/flex/row';
import { IOptionItem, OptionItem, RadioGroup, SelectDate } from 'components/form';
import { FormikCheckbox, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';

import { expireOptions, summaryOptions, toningOptions } from './constants';
import { IContentForm } from './interfaces';

export interface IContentSubForms {
  setContent: (content: IContentForm) => void;
  content: IContentForm;
}

export const PropertiesContentForm: React.FC<IContentSubForms> = ({ setContent, content }) => {
  const [{ series: lSeries }] = useLookup();
  const { values } = useFormikContext<IContentForm>();

  const [series, setSeries] = React.useState<IOptionItem[]>([]);

  const formatTime = (date: string) => {
    const converted = new Date(date);
    return !!converted.getTime()
      ? `${converted.getHours()}:${converted.getMinutes()}:${converted.getSeconds()}`
      : '';
  };

  React.useEffect(() => {
    setSeries(lSeries.map((m: any) => new OptionItem(m.name, m.id)));
  }, [lSeries]);

  return (
    <Col style={{ margin: '3%' }}>
      <Row>
        <Col>
          <Row>
            <FormikSelect
              className="md"
              isDisabled
              value={series && series.find((s: any) => s.value === values.seriesId)}
              onChange={(e: any) => setContent({ ...content, seriesId: e.value })}
              options={series}
              name="seriesId"
              label="Series"
            />
            <FormikText disabled className="md" name="otherSeries" label="Other Series" />
          </Row>
          <Row>
            <FormikSelect className="md" isDisabled name="eod" label="EoD Category" options={[]} />
            <FormikSelect isDisabled className="md" name="score" label="Score" options={[]} />
          </Row>
          <Row style={{ position: 'relative' }}>
            <Col>
              <SelectDate
                className="md-lrg"
                name="date"
                label="Date"
                selectedDate={values.publishedOn ?? ''}
                value={values.publishedOn}
                disabled
                onChange={(date: any) => {
                  setContent({ ...content, publishedOn: date });
                }}
              />
            </Col>
            <Col style={{ marginTop: '0.45em' }}>
              <FormikText
                disabled
                value={values.publishedOn ? formatTime(values.publishedOn!) : ''}
                name="time"
                label="Time"
              />
            </Col>
          </Row>
          <Row>
            <FormikText
              name="page"
              label="Page"
              onChange={(e: any) => setContent({ ...content, page: e.target.value })}
            />
          </Row>
        </Col>
        <Row style={{ marginLeft: '10%', marginTop: '1%' }}>
          <Col>
            <RadioGroup disabled spaceUnderRadio name="test" options={expireOptions} />
          </Col>
          <Col>
            <FormikCheckbox disabled className="chk" name="otherSnippet" label="Other Snippet" />
          </Col>
        </Row>
      </Row>
      <Row>
        <RadioGroup disabled direction="row" name="test" options={summaryOptions} />
      </Row>
      <Row>
        <FormikTextArea
          name="summary"
          label="Summary"
          value={values.summary}
          onChange={(e: any) => setContent({ ...content, summary: e.target.value })}
          style={{ width: '1000px', height: '400px' }}
        />
      </Row>
      <Row style={{ marginTop: '2%' }}>
        <RadioGroup disabled label="Toning" direction="row" name="toning" options={toningOptions} />
      </Row>
      <Row style={{ marginTop: '2%' }}>
        <Button disabled variant={ButtonVariant.action}>
          Attach File
        </Button>
        <Button disabled variant={ButtonVariant.danger}>
          Remove File
        </Button>
      </Row>
      <Row style={{ marginTop: '2%' }}>
        <FormikText disabled className="sm" name="prep" label="Prep Time" />
        <Button
          style={{ height: '40px', marginTop: '1.16em', marginRight: '2%' }}
          variant={ButtonVariant.action}
          disabled
        >
          Add
        </Button>
        <FormikText disabled className="sm" name="total" label="Total" />
        <Button
          disabled
          style={{ height: '40px', marginTop: '1.16em' }}
          variant={ButtonVariant.action}
        >
          View Log
        </Button>
      </Row>
    </Col>
  );
};
