import React from 'react';
import { FaBinoculars, FaPaperPlane } from 'react-icons/fa';
import { MdAdd } from 'react-icons/md';
import { useEveningOverviews } from 'store/hooks/admin/useEveningOverviews';
import { Button, ButtonVariant, Row } from 'tno-core';

import { IEveningOverviewSection } from './interfaces';
import { OverviewSection } from './overview-section';
import * as styled from './styled';

/** Evening overview section, contains table of items, and list of overview sections */
export const EveningOverview: React.FC = () => {
  const handleAdd = () => {
    setSections((prev) => [
      ...prev,
      {
        name: 'New section',
        items: [],
      },
    ]);
  };
  const [sections, setSections] = React.useState<IEveningOverviewSection[]>([]);
  const [, api] = useEveningOverviews();
  React.useEffect(() => {
    if (!sections.length) {
      api.findAllOverviewSections().then((data) => {
        setSections([...data]);
      });
    }
  }, [api, sections]);

  // console.log(sec)
  return (
    <styled.EveningOverview>
      <Row className="page-header">
        <div className="title">Evening Overview</div>
        <div className="buttons">
          <Button>
            Preview <FaBinoculars className="icon" />
          </Button>
          <Button>
            Publish <FaPaperPlane className="icon" />
          </Button>
        </div>
      </Row>
      {sections.map((section) => (
        <OverviewSection key={section.id} currentSection={section} setSections={setSections} />
      ))}
      <Button variant={ButtonVariant.action} className="new-section" onClick={() => handleAdd()}>
        New broadcast section <MdAdd className="icon" />
      </Button>
    </styled.EveningOverview>
  );
};
