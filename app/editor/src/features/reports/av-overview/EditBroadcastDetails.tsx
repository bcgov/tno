import { getIn, useFormikContext } from 'formik';
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
  IAVOverviewInstanceModel,
  Row,
  Show,
  Text,
} from 'tno-core';

import * as styled from './styled';

export interface IEditBroadcastDetailsProps {
  open?: boolean;
  editable?: boolean;
  index: number;
}

/** The dropdown that appears when clicking the edit button on an overview section. */
export const EditBroadcastDetails: React.FC<IEditBroadcastDetailsProps> = ({
  open: initOpen = false,
  editable = true,
  index,
}) => {
  const { values, setFieldValue } = useFormikContext<IAVOverviewInstanceModel>();
  const [{ series, sources, sourceOptions, seriesOptions }] = useLookupOptions();

  const section = values.sections[index];
  const [otherSource, setOtherSource] = React.useState(section.sourceId ? '' : section.otherSource);
  const [open, setOpen] = React.useState<boolean>(initOpen);

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
        <Button className="edit-button" onClick={() => setOpen(!open)}>
          {open ? <FaEyeSlash className="minimize" /> : <MdEdit className="icon" />}
        </Button>
        <h3>
          {section.name
            ? section.name +
              (!!getIn(values, `sections.${index}.anchors`)
                ? ' - ' + getIn(values, `sections.${index}.anchors`)
                : '')
            : 'Update broadcast details'}
        </h3>
      </Row>
      <Show visible={open}>
        <Row className={`edit-contents ${open ? 'in' : 'out'}`} justifyContent="space-evenly">
          <FormikSelect
            name={`sections.${index}.sourceId`}
            label="Source"
            options={sourceOptions}
            width={FieldSize.Big}
            value={sourceOptions.find((x) => x.value === section.sourceId) ?? ''}
            onChange={(e: any) => changeSource(e?.value)}
            clearValue={undefined}
            isDisabled={!editable}
          />
          <Text
            name="other"
            label="Other"
            value={otherSource}
            onChange={(e) => {
              setOtherSource(e.target.value);
            }}
            disabled={!editable}
          />
          <FormikSelect
            name={`sections.${index}.seriesId`}
            label="Show/Program"
            value={seriesOptions.find((x) => x.value === section.seriesId)}
            width={FieldSize.Big}
            onChange={(e: any) => changeSeries(e?.value)}
            options={seriesOptions}
            clearValue={undefined}
            isDisabled={!editable}
          />
          <FormikText label="Anchor(s)" name={`sections.${index}.anchors`} disabled={!editable} />
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
            disabled={!editable}
          />
        </Row>
      </Show>
    </styled.EditBroadcastDetails>
  );
};
