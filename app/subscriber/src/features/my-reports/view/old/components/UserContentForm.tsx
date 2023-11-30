import { IReportForm } from 'features/my-reports/interfaces';
import { getTime, toForm } from 'features/my-reports/utils';
import { useFormikContext } from 'formik';
import moment from 'moment';
import React from 'react';
import { FaSave } from 'react-icons/fa';
import { useContent, useLookup, useReports } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  FieldSize,
  FormikDatePicker,
  FormikSentiment,
  FormikText,
  FormikTextArea,
  FormikWysiwyg,
  IContentModel,
  IReportInstanceContentModel,
  Loading,
  Row,
  Show,
  TimeInput,
} from 'tno-core';

export interface IUserContentFormProps extends React.HTMLAttributes<HTMLDivElement> {
  /** The report instance content index (not the section index). */
  index: number;
  /** Which parts of the form to display */
  show: 'all' | 'summary' | 'none';
}

/**
 * Provides a small form to edit custom content a user owns.
 * @param param0 Component properties.
 * @returns A component.
 */
export const UserContentForm: React.FC<IUserContentFormProps> = ({
  index,
  show = 'none',
  className,
  ...rest
}) => {
  const { values, setFieldValue, setValues } = useFormikContext<IReportForm>();
  const [, { addContent, updateContent }] = useContent();
  const [{ updateReport }] = useReports();
  const [{ tonePools }] = useLookup();

  const [loading, setLoading] = React.useState(false);

  const saveContent = React.useCallback(
    async (index: number, content: IContentModel) => {
      try {
        setLoading(true);
        const originalId = content.id;
        const contentResult = !content.id
          ? await addContent(content)
          : await updateContent(content);
        if (contentResult) {
          const instanceContent: IReportInstanceContentModel = {
            ...values.instances[0].content[index],
            contentId: contentResult.id,
            content: contentResult,
          };
          setFieldValue(`instances.0.content.${index}`, instanceContent);

          const report = {
            ...values,
            instances: values.instances.map((instance) => ({
              ...instance,
              content: instance.content.map((ic, i) => (i === index ? instanceContent : ic)),
            })),
          };

          if (!originalId) {
            // Update the report instances with the latest content.
            const reportResult = await updateReport(report, true);
            setValues(toForm(reportResult));
          }
        }
      } catch {
      } finally {
        setLoading(false);
      }
    },
    [addContent, setFieldValue, setValues, updateContent, updateReport, values],
  );

  const content = values.instances[0].content[index].content;
  if (!content) return null;

  return show === 'none' ? null : (
    <Col className={`edit-content${className ? ` ${className}` : ''}`} {...rest}>
      <Show visible={loading}>
        <Loading />
      </Show>
      <Show visible={show === 'all'}>
        <FormikTextArea
          name={`instances.0.content.${index}.content.headline`}
          label="Headline"
          rows={1}
          required
        />
        <FormikText
          name={`instances.0.content.${index}.content.sourceUrl`}
          label="URL"
          onChange={(e) => {
            setFieldValue(`instances.0.content.${index}.content.sourceUrl`, e.target.value);
            setFieldValue(`instances.0.content.${index}.content.uid`, e.target.value);
          }}
        />
        <FormikText
          name={`instances.0.content.${index}.content.otherSource`}
          label="Source"
          required
        />
        <FormikText name={`instances.0.content.${index}.content.byline`} label="Byline" />
        <Row gap="0.5rem">
          <FormikDatePicker
            name={`instances.0.content.${index}.content.publishedOn`}
            label="Published On"
            required
            autoComplete="false"
            width={FieldSize.Medium}
            selectedDate={
              !!content.publishedOn ? moment(content.publishedOn).toString() : undefined
            }
            value={!!content.publishedOn ? moment(content.publishedOn).format('MMM D, yyyy') : ''}
            onChange={(date) => {
              setFieldValue(
                `instances.0.content.${index}.content.publishedOn`,
                moment(date).toISOString(),
              );
            }}
          />
          <TimeInput
            name={`instances.0.content.${index}.content.publishedOn`}
            label="Time"
            disabled={!content.publishedOn}
            width="7em"
            value={!!content.publishedOn ? moment(content.publishedOn).format('HH:mm:ss') : ''}
            placeholder={'HH:MM:SS'}
            onBlur={(e) => {
              if (e.target.value.indexOf('_')) {
                e.target.value = e.target.value.replaceAll('_', '0');
              }
              const date = getTime(e, content.publishedOn);
              if (!!date) {
                setFieldValue(
                  `instances.0.content.${index}.content.publishedOn`,
                  moment(date).toISOString(),
                );
              }
            }}
            onChange={(e) => {
              const date = getTime(e, content.publishedOn);
              if (!!date) {
                setFieldValue(
                  `instances.0.content.${index}.content.publishedOn`,
                  moment(date).format('MMM D, yyyy HH:mm:ss'),
                );
              }
            }}
          />
        </Row>
      </Show>
      <Show visible={['all', 'summary'].includes(show)}>
        <FormikWysiwyg name={`instances.0.content.${index}.content.summary`} label="Summary" />
      </Show>
      <Show visible={show === 'all'}>
        <FormikWysiwyg name={`instances.0.content.${index}.content.body`} label="Body" />
        <FormikSentiment
          name={`instances.0.content.${index}.content.tonePools`}
          options={tonePools}
        />
      </Show>
      <Row justifyContent="flex-end" className="bottom-actions">
        <Button variant={ButtonVariant.primary} onClick={() => saveContent(index, content)}>
          <Row alignItems="center" gap="1rem">
            <FaSave />
            Upload Story
          </Row>
        </Button>
      </Row>
    </Col>
  );
};
