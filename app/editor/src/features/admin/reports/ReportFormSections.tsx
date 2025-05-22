import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowDown, FaArrowUp, FaPlus, FaTrash } from 'react-icons/fa';
import { useFilters, useFolders } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Col,
  FormikCheckbox,
  FormikText,
  IReportModel,
  IReportSectionModel,
  ReportSectionTypeName,
  Row,
  Show,
} from 'tno-core';

import {
  ReportContentOptions,
  ReportHeadlineOptions,
  ReportOptions,
  ReportSectionContent,
  ReportSectionData,
  ReportSectionGallery,
  ReportSectionImage,
  ReportSectionMediaAnalytics,
  ReportSectionTableOfContents,
  ReportSectionText,
  SectionLabel,
} from './components';
import * as styled from './styled';
import { createReportSection } from './utils';

export const ReportFormSections = () => {
  const { values, setFieldValue, setValues } = useFormikContext<IReportModel>();
  const [{ filters }, { findFilters }] = useFilters();
  const [{ folders }, { findFolders }] = useFolders();

  React.useEffect(() => {
    if (!filters.length) findFilters({}).catch(() => {});
    if (!folders.length) findFolders({}).catch(() => {});
    // Only fetch items on initial load.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const [section, setSection] = React.useState<IReportSectionModel>();

  const handleAddSection = (type: ReportSectionTypeName) => {
    setFieldValue(
      'sections',
      [...values.sections, createReportSection(values.id, type)].map((section, index) => {
        return {
          ...section,
          sortOrder: index,
        };
      }),
    );
  };

  const handleMoveUp = (section: IReportSectionModel, index: number) => {
    var results = [...values.sections];
    var above = results[index - 1];
    results.splice(index, 1);
    results.splice(index - 1, 0, {
      ...section,
      sortOrder: above.sortOrder,
    });
    setFieldValue(
      'sections',
      results.map((r, i) => ({ ...r, sortOrder: i })),
    );
  };

  const handleMoveDown = (section: IReportSectionModel, index: number) => {
    var results = [...values.sections];
    var below = results[index + 1];
    results.splice(index, 1);
    results.splice(index + 1, 0, {
      ...section,
      sortOrder: below.sortOrder,
    });
    below.sortOrder--;
    setFieldValue(
      'sections',
      results.map((r, i) => ({ ...r, sortOrder: i })),
    );
  };

  const handleDelete = (index: number) => {
    if (!section) return;

    // Remove the content from the removed section in the current instance.
    const instance = values.instances.length ? { ...values.instances[0] } : undefined;
    if (instance) {
      const sectionNames = values.sections
        .filter((s) => s.name !== section.name)
        .map((s) => s.name);
      const content = instance
        ? instance.content.filter((c) => sectionNames.includes(c.sectionName))
        : [];
      instance.content = content;
    }

    if (index === section.sortOrder) setSection(undefined);
    setValues({
      ...values,
      sections: values.sections
        .filter((s, i) => i !== index)
        .map((section, index) => ({
          ...section,
          sortOrder: index,
        })),
      instances: instance
        ? values.instances.map((original, index) => (index === 0 ? instance : original))
        : values.instances,
    });
  };

  return (
    <styled.ReportSections>
      <h2>{values.name}</h2>
      <p>A report must contain one or more section to display content or charts.</p>
      <Row gap="1rem">
        <Col className="frm-in options" flex="1">
          <div>
            <label>Subject Line Options</label>
          </div>
          <Col>
            <p>Customize the email subject line.</p>
            <Row alignItems="center">
              <Col flex="1">
                <FormikText label="Text" name="settings.subject.text" required />
              </Col>
              <FormikCheckbox
                label="Show Today's Date"
                name="settings.subject.showTodaysDate"
                tooltip="Whether today's date will be included in the report subject line"
              />
            </Row>
          </Col>
        </Col>
        <ReportOptions />
      </Row>
      <Row gap="1rem">
        <ReportHeadlineOptions />
        <ReportContentOptions />
      </Row>
      <Col className="frm-in options">
        <Row>
          <label>Report Sections</label>
          <Row flex="1" gap="0.5rem" justifyContent="flex-end" alignItems="center">
            <FaPlus />
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => handleAddSection(ReportSectionTypeName.TableOfContents)}
            >
              Table of Contents
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => handleAddSection(ReportSectionTypeName.Text)}
            >
              Text
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => handleAddSection(ReportSectionTypeName.Content)}
            >
              Media Stories
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => handleAddSection(ReportSectionTypeName.MediaAnalytics)}
            >
              Media Analytics
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => handleAddSection(ReportSectionTypeName.Gallery)}
            >
              Gallery
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => handleAddSection(ReportSectionTypeName.Image)}
            >
              Image
            </Button>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => handleAddSection(ReportSectionTypeName.Data)}
            >
              Data
            </Button>
          </Row>
        </Row>
        <Col>
          <Col>
            <Col className="section-table">
              <Row className="row-header">
                <Col className="st-1">Type</Col>
                <Col className="st-2">Enable</Col>
                <Col className="st-3"></Col>
              </Row>
              {values.sections.map((row, index) => (
                <React.Fragment key={index}>
                  <Row
                    className={`row${row.sortOrder === section?.sortOrder ? ' active' : ''}`}
                    onClick={() => {
                      if (row.sortOrder !== section?.sortOrder) setSection(row);
                      else setSection(undefined);
                    }}
                  >
                    <Col className="st-1">
                      <SectionLabel section={row} />
                    </Col>
                    <Col className="st-2">
                      <FormikCheckbox name={`sections.${index}.isEnabled`} />
                    </Col>
                    <Col className="st-3">
                      <Col>
                        <Button
                          variant={ButtonVariant.link}
                          className="move"
                          disabled={index < 1 || values.sections.length < index}
                          onClick={(e) => {
                            e.preventDefault();
                            e.stopPropagation();
                            handleMoveUp(row, index);
                          }}
                        >
                          <FaArrowUp />
                        </Button>
                        <Button
                          variant={ButtonVariant.link}
                          className="move"
                          disabled={index >= values.sections.length - 1}
                          onClick={async (e) => {
                            e.preventDefault();
                            e.stopPropagation();
                            handleMoveDown(row, index);
                          }}
                        >
                          <FaArrowDown />
                        </Button>
                      </Col>
                      <Button
                        variant={ButtonVariant.danger}
                        onClick={async (e) => {
                          e.preventDefault();
                          e.stopPropagation();
                          handleDelete(index);
                        }}
                      >
                        <FaTrash />
                      </Button>
                    </Col>
                  </Row>
                  {!!section && section.sortOrder === row.sortOrder && (
                    <>
                      <Show visible={section.sectionType === ReportSectionTypeName.TableOfContents}>
                        <ReportSectionTableOfContents index={index} />
                      </Show>
                      <Show visible={section.sectionType === ReportSectionTypeName.Text}>
                        <ReportSectionText index={index} />
                      </Show>
                      <Show visible={section.sectionType === ReportSectionTypeName.Content}>
                        <ReportSectionContent index={index} />
                      </Show>
                      <Show visible={section.sectionType === ReportSectionTypeName.MediaAnalytics}>
                        <ReportSectionMediaAnalytics index={index} />
                      </Show>
                      <Show visible={section.sectionType === ReportSectionTypeName.Gallery}>
                        <ReportSectionGallery index={index} />
                      </Show>
                      <Show visible={section.sectionType === ReportSectionTypeName.Image}>
                        <ReportSectionImage index={index} />
                      </Show>
                      <Show visible={section.sectionType === ReportSectionTypeName.Data}>
                        <ReportSectionData index={index} />
                      </Show>
                    </>
                  )}
                </React.Fragment>
              ))}
            </Col>
          </Col>
        </Col>
      </Col>
    </styled.ReportSections>
  );
};
