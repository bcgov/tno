import { IOptionItem, OptionItem, RadioGroup, TimeInput } from 'components/form';
import { FormikRadioGroup, FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { FormikDatePicker } from 'components/formik/datepicker';
import { Modal } from 'components/modal/Modal';
import { IFile, Upload } from 'components/upload';
import { useFormikContext } from 'formik';
import { ContentType, IUserModel } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import moment from 'moment';
import React from 'react';
import { useContent, useLookup } from 'store/hooks';
import { Button, ButtonVariant, FieldSize, Show, useKeycloakWrapper } from 'tno-core';
import { Col } from 'tno-core/dist/components/flex/col';
import { Row } from 'tno-core/dist/components/flex/row';
import { getSortableOptions } from 'utils';

import { toningOptions } from './constants';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import { TimeLogTable } from './TimeLogTable';
import { getTotalTime } from './utils';

const tagMatch = /(?<=\[).+?(?=\])/g;
export interface IContentSummaryFormProps {
  setContent: (content: IContentForm) => void;
  content: IContentForm;
  contentType: ContentType;
}

/**
 * ContentSummaryForm component provides a form for content summary details.
 * @param param0 Component properties
 * @returns A new instance of a component.
 */
export const ContentSummaryForm: React.FC<IContentSummaryFormProps> = ({
  setContent,
  content,
  contentType,
}) => {
  const keycloak = useKeycloakWrapper();
  const [{ series, categories, licenses, tags, users }] = useLookup();
  const { values, setFieldValue, handleChange } = useFormikContext<IContentForm>();
  const { isShowing, toggle } = useModal();
  const [, { download }] = useContent();

  const [categoryOptions, setCategoryOptions] = React.useState<IOptionItem[]>([]);
  const [seriesOptions, setSeriesOptions] = React.useState<IOptionItem[]>([]);
  const [licenseOptions, setLicenseOptions] = React.useState<IOptionItem[]>([]);
  const [effort, setEffort] = React.useState(0);
  const [publishedOnTime, setPublishedOnTime] = React.useState<string>();

  const userId = users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id;

  const path = values.fileReferences.length ? values.fileReferences[0].path : undefined;
  const file = values.fileReferences.length
    ? ({
        name: values.fileReferences[0].fileName,
        size: values.fileReferences[0].size,
      } as IFile)
    : undefined;
  const [streamUrl, setStreamUrl] = React.useState<string>('');
  const videoRef = React.useRef<HTMLVideoElement>(null);

  React.useEffect(() => {
    setEffort(getTotalTime(values.timeTrackings));
  }, [values.timeTrackings]);

  React.useEffect(() => {
    const date = new Date(values.publishedOn);
    const hours = publishedOnTime?.split(':');
    if (!!hours && !publishedOnTime?.includes('_') && publishedOnTime !== '') {
      date.setHours(Number(hours[0]), Number(hours[1]), Number(hours[2]));
      setPublishedOnTime('');
      setFieldValue('publishedOn', date);
    }
  }, [publishedOnTime, setFieldValue, values.publishedOn]);

  React.useEffect(() => {
    setCategoryOptions(getSortableOptions(categories));
  }, [categories]);

  React.useEffect(() => {
    setSeriesOptions(series.map((m: any) => new OptionItem(m.name, m.id)));
  }, [series]);

  React.useEffect(() => {
    setLicenseOptions(licenses.map((m: any) => new OptionItem(m.name, m.id)));
  }, [licenses]);

  React.useEffect(() => {
    if (!!streamUrl && !!videoRef.current) {
      videoRef.current.src = streamUrl;
    }
  }, [streamUrl, videoRef]);

  const extractTags = (values: string[]) => {
    return tags
      .filter((tag) => values.some((value: string) => value.toLowerCase() === tag.id.toLowerCase()))
      .map((tag) => ({ ...tag }));
  };

  const setMedia = () => {
    setStreamUrl(!!streamUrl ? '' : `/api/editor/contents/upload/stream?path=${path}`);
  };

  return (
    <styled.ContentSummaryForm>
      <Col className="content-properties">
        <Row>
          <Col>
            <Show visible={contentType === ContentType.Snippet}>
              <Row>
                <FormikSelect
                  name="seriesId"
                  label="Series"
                  width={FieldSize.Medium}
                  value={seriesOptions.find((s: any) => s.value === values.seriesId) ?? ''}
                  options={seriesOptions}
                  isDisabled={!!values.otherSeries}
                  onChange={(e) => {
                    setFieldValue('otherSeries', '');
                  }}
                />
                <FormikText
                  name="otherSeries"
                  label="Other Series"
                  width={FieldSize.Medium}
                  onChange={(e) => {
                    const value = e.currentTarget.value;
                    setFieldValue('otherSeries', value);
                    if (!!value) setFieldValue('seriesId', undefined);
                  }}
                  onBlur={() => {
                    const found = series.find(
                      (s) => s.name.toLocaleLowerCase() === values.otherSeries.toLocaleLowerCase(),
                    );
                    if (!!found) {
                      setFieldValue('seriesId', found.id);
                      setFieldValue('otherSeries', '');
                    }
                  }}
                />
              </Row>
            </Show>
            <Row alignContent="flex-start" alignItems="flex-start">
              <FormikSelect
                name="categories"
                label="Event of Day Category"
                width={FieldSize.Medium}
                options={categoryOptions}
                clearValue={[]}
                value={
                  values.categories.length
                    ? categoryOptions.find((c) => c.value === values.categories[0].id) ?? []
                    : []
                }
                onChange={(e: any) => {
                  // only supports one at a time right now
                  const value = categories.find((c) => c.id === e.value);
                  setFieldValue('categories', !!value ? [value] : []);
                }}
              />
              <FormikText
                name="categories[0].score"
                label="Score"
                type="number"
                width={FieldSize.Medium}
                disabled={!values.categories.length}
              />
            </Row>
            <Row>
              <FormikDatePicker
                name="publishedOn"
                label="Published On"
                required
                autoComplete="false"
                width={FieldSize.Medium}
                selectedDate={
                  !!values.publishedOn ? moment(values.publishedOn).toString() : undefined
                }
                value={
                  !!values.publishedOn
                    ? moment(values.publishedOn).format('MMMM D, yyyy HH:mm:ss')
                    : ''
                }
                onChange={(date: any) => {
                  setFieldValue('publishedOn', date);
                }}
              />
              <TimeInput
                name="publishedOnTime"
                disabled={!values.publishedOn}
                placeholder={
                  !!values.publishedOn ? moment(values.publishedOn).format('HH:mm:ss') : 'HH:MM:SS'
                }
                label="Time"
                onChange={(e) => setPublishedOnTime(e.target.value)}
              />
              <Show visible={contentType === ContentType.Snippet}>
                <FormikText name="page" label="Page" onChange={handleChange} />
              </Show>
            </Row>
          </Col>
          <Col className="licenses">
            <RadioGroup
              label="License"
              spaceUnderRadio
              name="expireOptions"
              options={licenseOptions}
              value={licenseOptions.find((e) => e.value === values?.licenseId)}
              onChange={(e) => setFieldValue('licenseId', Number(e.target.value))}
            />
          </Col>
        </Row>
        <Row>
          <FormikTextArea
            name="summary"
            label="Summary"
            width={FieldSize.Stretch}
            required
            value={values.summary}
            onChange={handleChange}
            onBlur={(e) => {
              const value = e.currentTarget.value;
              if (!!value) {
                const values = value.match(tagMatch)?.toString()?.split(', ') ?? [];
                const tags = extractTags(values);
                setFieldValue('tags', tags);
              }
            }}
          />
        </Row>
        <Row>
          <FormikText
            disabled
            width={FieldSize.Large}
            name="tags"
            label="Tags"
            value={values.tags.map((t) => t.id).join(', ')}
          />
          <Button
            variant={ButtonVariant.danger}
            className="top-spacer"
            onClick={() => {
              const regex = /\[.*\]/; // TODO: This is far too eager and could remove valuable content.
              setFieldValue('summary', values.summary.replace(regex, ''));
              setFieldValue('tags', []);
            }}
          >
            Clear Tags
          </Button>
        </Row>
        <Row className="row-margins">
          <FormikRadioGroup
            label="Toning"
            direction="row"
            name="tonePool"
            required
            options={toningOptions}
            onChange={(e, value) => {
              setFieldValue('tonePool', value);
              setFieldValue('tone', value?.value);
            }}
          />
        </Row>
        <Show visible={contentType === ContentType.Snippet}>
          <Row className="row-margins">
            <Upload
              id="upload"
              name="file"
              file={file}
              onSelect={(e) => {
                const file = !!e.target?.files?.length ? e.target.files[0] : undefined;
                setFieldValue('file', file);
                // Remove file reference.
                setFieldValue('fileReferences', []);
              }}
              onDownload={() => {
                download(values.id, file?.name ?? `${values.source}-${values.id}`);
              }}
              onDelete={() => {
                setStreamUrl('');
                if (!!videoRef.current) {
                  videoRef.current.src = '';
                }
              }}
            />
            <Button
              onClick={() => {
                setMedia();
              }}
              variant={ButtonVariant.secondary}
              className={!file ? 'hidden' : 'show-player'}
            >
              {!!streamUrl ? 'Hide Player' : 'Show Player'}
            </Button>
          </Row>
          <Col className="video" alignItems="stretch">
            <video ref={videoRef} className={!streamUrl ? 'hidden' : ''} controls>
              HTML5 Video is required for this example
            </video>
          </Col>
          <Row className="row-margins">
            <FormikText className="sm" name="prep" label="Prep Time (minutes)" type="number" />
            <Button
              className="top-spacer add-time"
              variant={ButtonVariant.secondary}
              disabled={isNaN((values as any).prep)}
              onClick={() => {
                setEffort(effort!! + Number((values as any).prep));
                setFieldValue('timeTrackings', [
                  ...values.timeTrackings,
                  {
                    userId: userId ?? 0,
                    activity: !!values.id ? 'Updated' : 'Created',
                    effort: (values as any).prep,
                    createdOn: new Date(),
                  },
                ]);
                setFieldValue('prep', '');
              }}
            >
              Add
            </Button>
            <FormikText
              disabled
              className="sm"
              name="total"
              label="Total"
              value={effort?.toString()}
            />
            <Button
              onClick={() => {
                setContent({ ...content, timeTrackings: values.timeTrackings });
                toggle();
              }}
              className="top-spacer"
              variant={ButtonVariant.secondary}
            >
              View Log
            </Button>
            <Modal
              hide={toggle}
              isShowing={isShowing}
              headerText="Prep Time Log"
              body={
                <TimeLogTable
                  setTotalEffort={setEffort}
                  totalEffort={effort}
                  data={values.timeTrackings}
                />
              }
              customButtons={
                <Button variant={ButtonVariant.secondary} onClick={toggle}>
                  Close
                </Button>
              }
            />
          </Row>
        </Show>
      </Col>
    </styled.ContentSummaryForm>
  );
};
