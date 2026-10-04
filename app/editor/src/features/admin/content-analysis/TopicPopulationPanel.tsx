import React from 'react';
import { toast } from 'react-toastify';
import { useTopicPopulation } from 'store/hooks/admin';
import { Button, Col, OptionItem, Row, Select } from 'tno-core';

import { type ITopicPopulationSettingsModel, TopicPopulationModeName } from './interfaces';

const modeOptions = [
  new OptionItem('Existing active topics only', TopicPopulationModeName.ExistingOnly),
  new OptionItem('Allow creating topics', TopicPopulationModeName.AllowCreate),
];

/**
 * Controls whether Content-Analysis may create topics when it assigns them. Whether it assigns
 * topics at all is configured on the Content-Analysis service (its Topics process).
 * @returns Component.
 */
export const TopicPopulationPanel: React.FC = () => {
  const { getPopulationSettings, updatePopulationSettings } = useTopicPopulation();
  const [saved, setSaved] = React.useState<ITopicPopulationSettingsModel>();
  const [values, setValues] = React.useState<ITopicPopulationSettingsModel>({
    mode: TopicPopulationModeName.ExistingOnly,
  });

  React.useEffect(() => {
    getPopulationSettings()
      .then((settings) => {
        setSaved(settings);
        setValues(settings);
      })
      .catch(() => {});
  }, [getPopulationSettings]);

  const isChanged = !!saved && saved.mode !== values.mode;

  const handleSave = async () => {
    try {
      const result = await updatePopulationSettings(values);
      setSaved(result);
      setValues(result);
      toast.success('Topic population settings saved.');
    } catch {}
  };

  return (
    <Col className="panel" gap="0.5rem">
      <h2>Automatic topics</h2>
      <Row gap="1rem" alignItems="flex-end" className="field-row">
        <Select
          name="topicPopulationMode"
          label="Topic population"
          width="28ch"
          options={modeOptions}
          value={modeOptions.find((o) => o.value === values.mode)}
          onChange={(o) =>
            setValues({
              ...values,
              mode: ((o as OptionItem)?.value as TopicPopulationModeName) ?? values.mode,
            })
          }
        />
        <Button onClick={handleSave} disabled={!isChanged}>
          Save
        </Button>
      </Row>
      <p className="hint">
        Applies when the Content-Analysis service runs its Topics process. Only stories without
        topics are given one. Scores come from the topic score rules.
      </p>
    </Col>
  );
};
