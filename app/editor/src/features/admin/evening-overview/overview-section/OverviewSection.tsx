import React from 'react';
import { FaEyeSlash, FaMinus, FaSave, FaSpinner } from 'react-icons/fa';
import { MdAdd, MdClear, MdEdit, MdMinimize, MdSave } from 'react-icons/md';
import { useLookupOptions } from 'store/hooks';
import { useEveningOverviews } from 'store/hooks/admin/useEveningOverviews';
import { Button, ButtonVariant, Row, Show, Spinner } from 'tno-core';

import {
  EveningOverviewItemType,
  IEveningOverviewItem,
  IEveningOverviewSection,
} from '../interfaces';
import { OverviewGrid } from '../overview-grid';
import { EditBroadcastDetails } from './EditBroadcastDetails';
import * as styled from './styled';

export interface IOverviewSectionProps {
  currentSection?: IEveningOverviewSection;
  setSections: React.Dispatch<React.SetStateAction<IEveningOverviewSection[]>>;
}

/** This section includes the overview grid, as well as the broadcast details. */
export const OverviewSection: React.FC<IOverviewSectionProps> = ({
  currentSection,
  setSections,
}) => {
  const [items, setItems] = React.useState<IEveningOverviewItem[]>([]);
  const [editable, setEditable] = React.useState<boolean>(false);
  const [section, setSection] = React.useState<IEveningOverviewSection>();
  const [saving, setSaving] = React.useState<boolean>(false);

  const [, api] = useEveningOverviews();
  React.useEffect(() => {
    if (currentSection?.id && !items.length) {
      api.findItemsBySectionId(currentSection.id).then((result) => {
        setItems(result);
      });
    }
  }, [currentSection]);

  // sort items based on sortorder when ever new items are retrieved or added
  React.useEffect(() => {
    setItems((prev) => {
      return prev.slice().sort((a, b) => a.sortOrder - b.sortOrder);
    });
  }, [items.length]);

  React.useEffect(() => {
    if (currentSection) {
      setSection(currentSection);
    }
  }, [currentSection]);

  const [{ sourceOptions, seriesOptions }] = useLookupOptions();

  const handleAdd = () => {
    setItems((prev) => [
      ...prev,
      {
        itemType: !prev.length
          ? EveningOverviewItemType.Intro.id
          : EveningOverviewItemType.Story.id,
        time: '18:00:00',
        summary: '',
        name: section?.name ?? '',
        avOverviewSectionId: section?.id,
        sortOrder: prev.length,
      },
    ]);
  };

  const handleClear = () => {
    const temp: IEveningOverviewItem[] = items.map((x) => ({ ...x, summary: '' }));
    setItems(temp);
  };

  const handleSaveItems = async () => {
    setSaving(true);
    items.forEach(async (item, index) => {
      console.log(item);
      try {
        if (item.id) {
          await api.updateOverviewSectionItem({ ...item, sortOrder: index });
        } else {
          await api.addOverviewSectionItem({ ...item, sortOrder: index });
        }
      } catch {
      } finally {
        if (index === items.length - 1) {
          setSaving(false);
        }
      }
    });
  };

  return (
    <styled.OverviewSection className="section">
      <div className="section">
        <Row className="title-edit">
          <Button className="edit-button" onClick={() => setEditable(!editable)}>
            {editable ? <FaEyeSlash className="minimize" /> : <MdEdit className="icon" />}
          </Button>
          <div className="section-title">
            {seriesOptions.find((x) => x.value === section?.seriesId)?.label}
          </div>
        </Row>
        <Show visible={editable}>
          <EditBroadcastDetails
            editable={editable}
            currentSection={section}
            items={items}
            setSections={setSections}
            setSection={setSection}
          />
        </Show>
        <OverviewGrid key={currentSection?.id} setItems={setItems} items={items} />
        <Row className="actions buttons">
          <Button
            disabled={!section?.id}
            variant={ButtonVariant.action}
            className="new-story"
            onClick={() => handleAdd()}
          >
            New story <MdAdd className="icon" />
          </Button>
          <Button variant={ButtonVariant.danger} className="clear" onClick={() => handleClear()}>
            Clear all story text <MdClear className="icon" />
          </Button>
          <Button
            onClick={() => handleSaveItems()}
            className="save-items"
            variant={ButtonVariant.success}
          >
            Save items
            {saving ? <FaSpinner className="icon spinner" /> : <MdSave className="icon" />}
          </Button>
        </Row>
      </div>
    </styled.OverviewSection>
  );
};
