import { set } from 'lodash';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useLookupOptions } from 'store/hooks';
import { useEveningOverviews } from 'store/hooks/admin/useEveningOverviews';
import { Button, ButtonVariant, Col, FieldSize, Row, Select, Show, Text } from 'tno-core';

import { IEveningOverviewItem, IEveningOverviewSection } from '../interfaces';
import * as styled from './styled';

export interface IEditBroadcastDetailsProps {
  editable: boolean;
  currentSection?: IEveningOverviewSection;
  setSection: (section: IEveningOverviewSection) => void;
  setSections: React.Dispatch<React.SetStateAction<IEveningOverviewSection[]>>;
  items: IEveningOverviewItem[];
}

/** The dropdown that appears when clicking the edit button on an overview section. */
export const EditBroadcastDetails: React.FC<IEditBroadcastDetailsProps> = ({
  currentSection,
  setSection,
  editable,
  setSections,
  items,
}) => {
  const [{ sourceOptions, seriesOptions }] = useLookupOptions();
  const [, api] = useEveningOverviews();

  const handleSave = async () => {
    try {
      if (currentSection && !currentSection.id) {
        const result = await api.addOverviewSection(currentSection);
        setSection(result);
        toast.success(`${result.name} has successfully been saved.`);
      }
      if (currentSection?.id) {
        const result = await api.updateOverviewSection(currentSection);
        setSection(result);
        toast.success(`${result.name} has successfully been saved.`);
      }
    } catch {}
  };

  const handleDelete = async () => {
    try {
      if (currentSection?.id) {
        const result = await api.deleteOverviewSection(currentSection);
        setSection(result);
        setSections((prev) => prev.slice().filter((x) => x.id !== result.id));
        toast.success(`${result.name} has successfully been deleted.`);
      }
    } catch {}
  };

  return (
    <styled.EditBroadcastDetails className={editable ? 'in' : 'out'}>
      <Row className="edit-header">
        <h3>Update broadcast details </h3>
      </Row>
      <div className="edit-contents">
        <Row justifyContent="space-evenly">
          <Select
            options={sourceOptions}
            width={FieldSize.Big}
            value={sourceOptions.find((x) => x.value === currentSection?.sourceId)}
            onChange={(e: any) => {
              setSection({
                ...currentSection,
                sourceId: e.value,
                name: (sourceOptions.find((x) => x.value === e.value)?.label as string) ?? '',
              });
            }}
            name="source"
            label="Source"
          />
          <Text
            name="other"
            label="Other"
            onChange={(e) => setSection({ ...currentSection, name: e.target.value })}
          />
          <Select
            width={FieldSize.Big}
            name="series"
            label="Series"
            value={seriesOptions.find((x) => x.value === currentSection?.seriesId)}
            onChange={(e: any) => {
              setSection({ ...currentSection, seriesId: e.value });
            }}
            options={seriesOptions}
          />
          <Text
            onChange={(e) => setSection({ ...currentSection, anchors: e.target.value })}
            name="anchors"
            value={currentSection?.anchors}
            label="Anchor(s)"
          />
          <Text
            onChange={(e) => setSection({ ...currentSection, startTime: e.target.value })}
            name="time"
            label="Time"
            value={currentSection?.startTime}
          />
        </Row>
        <Row className="tools">
          <div className="buttons">
            <Button
              variant={ButtonVariant.danger}
              onClick={() => handleDelete()}
              tooltip={!!items.length ? 'Delete all items before deleting section' : undefined}
              className="delete-button"
              disabled={!!items.length}
            >
              Delete section
              <FaTrash className="delete" />
            </Button>
            <Button onClick={() => handleSave()} className="save">
              Save
            </Button>
          </div>
        </Row>
      </div>
    </styled.EditBroadcastDetails>
  );
};
