import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import { useApp } from 'store/hooks';
import { Col, Show, TextArea, Wysiwyg } from 'tno-core';

export interface IContentFormProps extends React.HTMLAttributes<HTMLDivElement> {
  /** The report instance content index (not the section index). */
  index: number;
  /** Which parts of the form to display */
  show: 'all' | 'summary' | 'none';
}

/**
 * Provides a small form to edit custom content override values for content a user does not own.
 * @param param0 Component properties.
 * @returns A component.
 */
export const ContentForm: React.FC<IContentFormProps> = ({
  index,
  show = 'none',
  className,
  ...rest
}) => {
  const { values, setFieldValue } = useFormikContext<IReportForm>();
  const [{ userInfo }] = useApp();

  const content = values.instances[0].content[index].content;
  if (!content) return null;

  const userId = userInfo?.id ?? 0;
  const headline = content.versions?.[userId]?.headline
    ? content.versions[userId].headline ?? ''
    : content.headline;

  return show === 'none' ? null : (
    <Col className={`edit-content${className ? ` ${className}` : ''}`} {...rest}>
      <Show visible={show === 'all'}>
        <TextArea
          name={`instances.0.content.${index}.versions.${userId}.headline`}
          label="Headline"
          rows={1}
          onChange={(e) => {
            setFieldValue(`instances.0.content.${index}.content.versions`, {
              ...content.versions,
              [userId]: {
                ...content.versions?.[userId],
                headline: e.target.value,
              },
            });
          }}
          value={headline}
        />
      </Show>
      <Show visible={['all', 'summary'].includes(show)}>
        <Wysiwyg
          name={`instances.0.content.${index}.versions.${userId}.summary`}
          label="Summary"
          value={content.versions?.[userId]?.summary ?? content.summary}
          onChange={(text) =>
            setFieldValue(`instances.0.content.${index}.content.versions`, {
              ...content.versions,
              [userId]: {
                ...content.versions?.[userId],
                summary: text,
              },
            })
          }
        />
      </Show>
      <Show visible={show === 'all'}>
        <Wysiwyg
          name={`instances.0.content.${index}.versions.${userId}.body`}
          label="Body"
          value={content.versions?.[userId]?.body ?? content.body ?? ''}
          onChange={(text) => {
            setFieldValue(`instances.0.content.${index}.content.versions`, {
              ...content.versions,
              [userId]: {
                ...content.versions?.[userId],
                body: text,
              },
            });
          }}
        />
      </Show>
    </Col>
  );
};
