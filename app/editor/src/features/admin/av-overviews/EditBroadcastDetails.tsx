import { useFormikContext } from 'formik';
import React from 'react';
import { FaEyeSlash } from 'react-icons/fa';
import { MdEdit } from 'react-icons/md';
import { useLookupOptions } from 'store/hooks';
import {
  Button,
  FieldSize,
  FormikSelect,
  FormikText,
  FormikTimeInput,
  IAVOverviewTemplateModel,
  Row,
  Show,
  Text,
} from 'tno-core';

import * as styled from './styled';

export interface IEditBroadcastDetailsProps {
  editable?: boolean;
  index: number;
}

/** The dropdown that appears when clicking the edit button on an overview section. */
export const EditBroadcastDetails: React.FC<IEditBroadcastDetailsProps> = ({
  editable: initEditable = false,
  index,
}) => {
  const { values, setFieldValue } = useFormikContext<IAVOverviewTemplateModel>();
  const [{ series, sources, sourceOptions, seriesOptions }] = useLookupOptions();

  const section = values.sections[index];
  const [otherSource, setOtherSource] = React.useState(section.sourceId ? '' : section.otherSource);
  const [editable, setEditable] = React.useState<boolean>(initEditable);

  React.useEffect(() => {
    if (otherSource) {
      const item = series.find((s) => s.id === section.seriesId);
      setFieldValue(`sections.${index}`, {
        ...section,
        name: `${otherSource}${item?.name ? ` - ${item.name}` : ''}`,
        sourceId: undefined,
        otherSource: otherSource,
      });
    }
    // The 'section' causes infinite loop.
    // Only want to update when 'otherSource' changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [index, otherSource, series, setFieldValue]);

  const changeSource = React.useCallback(
    (id: number | undefined) => {
      const source = sources.find((s) => s.id === id);
      const item = series.find((s) => s.id === section.seriesId);
      if (source) {
        setFieldValue(`sections.${index}`, {
          ...section,
          name: `${source.code}${item?.name ? ` - ${item.name}` : ''}`,
          sourceId: source.id,
          otherSource: source.code,
        });
      } else {
        setFieldValue(`sections.${index}`, {
          ...section,
          name: item?.name,
          sourceId: undefined,
          otherSource: '',
        });
      }
      setOtherSource('');
    },
    [index, section, series, setFieldValue, sources],
  );

  const changeSeries = React.useCallback(
    (id: number | undefined) => {
      const source = sources.find((s) => s.id === section.sourceId);
      const item = series.find((s) => s.id === id);
      if (item) {
        const name = source?.code ?? section.otherSource;
        setFieldValue(`sections.${index}`, {
          ...section,
          name: `${name ? `${name} - ` : ''}${item.name}`,
          seriesId: item.id,
        });
      } else {
        setFieldValue(`sections.${index}`, {
          ...section,
          name: source?.name ?? section.otherSource,
          seriesId: undefined,
        });
      }
    },
    [index, section, series, setFieldValue, sources],
  );

  return (
    <styled.EditBroadcastDetails>
      <Row className="edit-header">
        <Button className="edit-button" onClick={() => setEditable(!editable)}>
          {editable ? <FaEyeSlash className="minimize" /> : <MdEdit className="icon" />}
        </Button>
        <h3>{section.name ? section.name : 'Update broadcast details'}</h3>
      </Row>
      <Show visible={editable}>
        <Row className={`edit-contents ${editable ? 'in' : 'out'}`} justifyContent="space-evenly">
          <FormikSelect
            name={`sections.${index}.sourceId`}
            label="Source"
            options={sourceOptions}
            width={FieldSize.Big}
            value={sourceOptions.find((x) => x.value === section.sourceId) ?? ''}
            onChange={(e: any) => changeSource(e?.value)}
            clearValue={undefined}
          />
          <Text
            name="other"
            label="Other"
            value={otherSource}
            onChange={(e) => {
              setOtherSource(e.target.value);
            }}
          />
          <FormikSelect
            name={`sections.${index}.seriesId`}
            label="Series"
            value={seriesOptions.find((x) => x.value === section.seriesId)}
            width={FieldSize.Big}
            onChange={(e: any) => changeSeries(e?.value)}
            options={seriesOptions}
            clearValue={undefined}
          />
          <FormikText label="Anchor(s)" name={`sections.${index}.anchors`} />
          <FormikTimeInput
            label="Start Time"
            name={`sections.${index}.startTime`}
            width={FieldSize.Small}
            placeholder="hh:mm:ss"
            onBlur={(e) => {
              setFieldValue(`sections.${index}`, {
                ...section,
                items: section.items.map((i) => (i.time ? i : { ...i, time: e.target.value })),
              });
            }}
          />
        </Row>
      </Show>
    </styled.EditBroadcastDetails>
  );
};
