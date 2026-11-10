import { defaultSource } from 'features/admin/sources/constants';
import React from 'react';
import { Link } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useSources } from 'store/hooks/admin';
import { Button, ButtonVariant, Col, type ISourceModel, Modal, OptionItem, Select } from 'tno-core';

import { type ITopicScoreSourceModel } from './interfaces';

interface IAddScoringSourceProps {
  sources: ITopicScoreSourceModel[];
  onAdded: (id: number) => Promise<void>;
  onClose: () => void;
}

/** Enable topic scoring for an existing source, or open the form to create a new source. */
export const AddScoringSource: React.FC<IAddScoringSourceProps> = ({
  sources,
  onAdded,
  onClose,
}) => {
  const [, api] = useSources();
  const [available, setAvailable] = React.useState<ISourceModel[]>([]);
  const [selectedId, setSelectedId] = React.useState<number>();
  const [loading, setLoading] = React.useState(true);
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    api
      .findAllSources()
      .then(setAvailable)
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [api]);

  const options = available
    .filter((s) => !sources.some((existing) => existing.id === s.id))
    .sort((a, b) => a.name.localeCompare(b.name))
    .map((s) => new OptionItem(`${s.name} (${s.code})`, s.id));

  const handleAdd = async () => {
    if (!selectedId || saving) return;
    setSaving(true);
    try {
      // Fetch the full, current model so other source settings and its version are preserved.
      const source = await api.getSource(selectedId);
      if (!source.useInTopics) await api.updateSource({ ...source, useInTopics: true });
      await onAdded(source.id);
      toast.success(`${source.name} added to topic scoring.`);
    } catch {
      // Errors are handled globally; leave the picker open so the user can retry.
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal
      isShowing
      type="custom"
      headerText="Add source to topic scoring"
      component={
        <Col gap="0.5rem">
          <p>Choose an existing source to enable Use in Topics, then add its scoring rules.</p>
          <Select
            name="scoringSource"
            label="Source"
            options={options}
            value={options.find((o) => o.value === selectedId)}
            isLoading={loading}
            isDisabled={loading || saving}
            menuPosition="fixed"
            onChange={(o) => setSelectedId((o as OptionItem)?.value as number | undefined)}
          />
          <p>
            Need a new source?{' '}
            <Link to="/admin/sources/0" state={{ source: { ...defaultSource, useInTopics: true } }}>
              Create a source
            </Link>{' '}
            with Use in Topics selected, then return here to configure its rules.
          </p>
        </Col>
      }
      customButtons={
        <>
          <Button onClick={handleAdd} disabled={!selectedId || loading || saving}>
            Add source
          </Button>
          <Button variant={ButtonVariant.secondary} onClick={onClose} disabled={saving}>
            Cancel
          </Button>
        </>
      }
    />
  );
};
