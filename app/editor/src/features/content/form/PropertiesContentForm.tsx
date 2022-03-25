import { IOptionItem, OptionItem, RadioGroup, SelectDate } from 'components/form';
import { FormikCheckbox, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { Modal } from 'components/modal/Modal';
import { Upload } from 'components/upload';
import { useFormikContext } from 'formik';
import { ITagModel, ITimeTrackingModel, IUserModel } from 'hooks/api-editor';
import useModal from 'hooks/modal/useModal';
import React from 'react';
import { useLookup } from 'store/hooks';
import { Button, ButtonVariant, useKeycloakWrapper } from 'tno-core';
import { Col } from 'tno-core/dist/components/flex/col';
import { Row } from 'tno-core/dist/components/flex/row';
import { getSortableOptions } from 'utils';

import { expireOptions, summaryOptions, toningOptions } from './constants';
import { IContentForm } from './interfaces';
import { TimeLogTable } from './TimeLogTable';

export interface IContentSubForms {
  setContent: (content: IContentForm) => void;
  content: IContentForm;
}

/** The sub form of the ContentForm when the Properties Tab is selected. */
export const PropertiesContentForm: React.FC<IContentSubForms> = ({ setContent, content }) => {
  const [{ series: lSeries, categories, tags, users }] = useLookup();
  const { values, setFieldValue, handleChange } = useFormikContext<IContentForm>();
  const [userTags, setUserTags] = React.useState<string[]>();
  const [categoryTypes, setCategoryTypes] = React.useState<IOptionItem[]>([]);
  const keycloak = useKeycloakWrapper();
  const userId = users.find((u: IUserModel) => u.displayName === keycloak.getDisplayName())?.id;

  React.useEffect(() => {
    setCategoryTypes(getSortableOptions(categories));
  }, [categories]);

  const { isShowing, toggle } = useModal();

  let timeLog = values.timeTrackings;
  const [series, setSeries] = React.useState<IOptionItem[]>();

  const formatTime = (date: string) => {
    const converted = new Date(date);
    return !!converted.getTime() ? converted.getTime() : '';
  };

  React.useEffect(() => {
    setSeries(lSeries.map((m: any) => new OptionItem(m.name, m.id)));
  }, [lSeries]);

  const tagMatch = /(?<=\[).+?(?=\])/g;
  let validTags;

  const getTotalTime = () => {
    let count = 0;
    if (values.timeTrackings.length === 0) {
      return 0;
    }
    values.timeTrackings.forEach((t: ITimeTrackingModel) => (count += Number(t.effort)));
    return count;
  };

  const [effort, setEffort] = React.useState(getTotalTime());

  /** capture user's tags when entering them in the summary field */
  React.useEffect(() => {
    var result;
    if (values.summary) {
      result = values.summary.match(tagMatch)?.toString();
      setUserTags(result?.split(', '));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values.summary]);

  /** add tags to formik value object if tag exists - better implementation later*/
  React.useEffect(() => {
    validTags = tags.filter((t1: ITagModel) => userTags?.some((t2: string) => t2 === t1.id));
    setFieldValue('tags', validTags);
  }, [userTags]);

  return (
    <Col style={{ margin: '3%' }}>
      <Row>
        <Col>
          <Row>
            <FormikSelect
              className="md"
              value={series && series.find((s: any) => s.value === values.seriesId)}
              onChange={(e: any) => {
                setFieldValue('seriesId', e.value);
              }}
              options={series!}
              name="seriesId"
              label="Series"
            />
            <FormikText disabled className="md" name="otherSeries" label="Other Series" />
          </Row>
          <Row>
            <FormikSelect className="md" isDisabled name="eod" label="EoD Category" options={categoryTypes} />
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
                onChange={(date: any) => {
                  setFieldValue('publishedOn', date);
                }}
              />
            </Col>
            <Col style={{ marginTop: '0.45em' }}>
              <FormikText
                disabled
                value={formatTime(values.publishedOn)}
                name="time"
                label="Time"
              />
            </Col>
          </Row>
          <Row>
            <FormikText name="page" label="Page" onChange={handleChange} />
          </Row>
        </Col>
        <Row style={{ marginLeft: '10%', marginTop: '1%' }}>
          <Col>
            <RadioGroup
              spaceUnderRadio
              name="expireOptions"
              options={expireOptions}
              value={expireOptions.find((e) => e.value === values?.licenseId)}
              onChange={(e) => setFieldValue('licenseId', Number(e.target.value))}
            />
          </Col>
          <Col>
            <FormikCheckbox disabled className="chk" name="otherSnippet" label="Other Snippet" />
          </Col>
        </Row>
      </Row>
      <Row>
        {/* Currently only support text. */}
        <RadioGroup
          disabled
          value={summaryOptions.find((x) => x.value === 0)}
          direction="row"
          name="test"
          options={summaryOptions}
        />
      </Row>
      <Row>
        <FormikTextArea
          name="summary"
          label="Summary"
          value={values.summary}
          onChange={handleChange}
          style={{ width: '1000px', height: '400px' }}
        />
      </Row>
      <Row>
        <FormikText disabled name="tags" label="Tags" value={userTags && userTags.join(', ')} />
        <Button
          variant={ButtonVariant.danger}
          style={{ marginTop: '1.16em' }}
          onClick={() => {
            const regex = /\[.*\]/;
            setFieldValue('summary', values.summary.replace(regex, ''));
            setUserTags([]);
          }}
        >
          Clear Tags
        </Button>
      </Row>
      <Row style={{ marginTop: '2%' }}>
        <RadioGroup disabled label="Toning" direction="row" name="toning" options={toningOptions} />
      </Row>
      <Row style={{ marginTop: '2%' }}>
        <Upload />
      </Row>
      <Row style={{ marginTop: '2%' }}>
        <FormikText className="sm" name="prep" label="Prep Time" />
        <Button
          style={{ marginTop: '1.16em', marginRight: '2%' }}
          variant={ButtonVariant.action}
          onClick={() => {
            setEffort(effort!! + Number((values as any).prep));
            timeLog.push({
              userId: userId ?? 0,
              activity: !!values.id ? 'Updated' : 'Created',
              effort: (values as any).prep,
              createdOn: new Date(),
            });
            setFieldValue('timeTrackings', timeLog);
          }}
        >
          Add
        </Button>
        <FormikText disabled className="sm" name="total" label="Total" value={effort?.toString()} />
        <Button
          onClick={() => {
            setContent({ ...content, timeTrackings: values.timeTrackings });
            toggle();
          }}
          style={{ marginTop: '1.16em' }}
          variant={ButtonVariant.action}
        >
          View Log
        </Button>
        <Modal
          hide={toggle}
          isShowing={isShowing}
          headerText="Prep Time Log"
          body={<TimeLogTable totalTime={effort} data={timeLog} />}
          customButtons={
            <Button variant={ButtonVariant.action} onClick={toggle}>
              Close
            </Button>
          }
        />
      </Row>
    </Col>
  );
};
