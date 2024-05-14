import { useFormikContext } from 'formik';
import React from 'react';
import { FaArrowDown, FaArrowUp, FaPlus, FaTrash } from 'react-icons/fa';
import { useFilters, useFolders } from 'store/hooks/admin';
import {
  Button,
  ButtonVariant,
  Checkbox,
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
  ReportSectionContent,
  ReportSectionGallery,
  ReportSectionMediaAnalytics,
  ReportSectionTableOfContents,
  ReportSectionText,
  SectionLabel,
} from './components';
import * as styled from './styled';
import { createReportSection } from './utils';

export const ReportFormSections = () => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();
  const [{ filters }, { findAllFilters }] = useFilters();
  const [{ folders }, { findAllFolders }] = useFolders();

  React.useEffect(() => {
    if (!filters.length) findAllFilters().catch(() => {});
    if (!folders.length) findAllFolders().catch(() => {});
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
    var sections = [...values.sections];
    sections.splice(index, 1);
    if (index === section?.sortOrder) setSection(undefined);
    setFieldValue(
      `sections`,
      sections.map((s, i) => ({
        ...s,
        sortOrder: i,
      })),
    );
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
        <Col className="frm-in options" flex="1">
          <div>
            <label>Report Options</label>
          </div>
          <Col>
            <p>
              Control the output of the report. Redirect users to view the report on the website
              instead of in their email.
            </p>
            <FormikCheckbox
              label="Show Link to Story"
              name="settings.content.showLinkToStory"
              tooltip="Include a link to view the story online"
            />
            <FormikCheckbox
              label="Highlight Keywords"
              name="settings.content.highlightKeywords"
              tooltip="Highlight the search keywords found in the report content"
            />
            <FormikCheckbox
              label="Use Page Breaks"
              name="settings.sections.usePageBreaks"
              tooltip="use page breaks in each section for printing."
            />
            <Checkbox
              name={`settings.content.copyPriorInstance`}
              label="Copy prior report when starting new report"
              checked={values.settings.content.copyPriorInstance}
              onChange={(e) => {
                setFieldValue('settings.content.copyPriorInstance', e.target.checked);
              }}
            />
            <Checkbox
              name={`settings.content.clearOnStartNewReport`}
              label="Empty report when starting next report"
              tooltip="If this is not set an auto report will not generate a new report if an active one already exists."
              checked={values.settings.content.clearOnStartNewReport}
              onChange={(e) => {
                setFieldValue('settings.content.clearOnStartNewReport', e.target.checked);
              }}
            />
            <Checkbox
              name={`settings.content.excludeContentInUnsentReport`}
              label="Exclude content found in unsent report"
              tooltip="Excluding content in the current unsent report ensures each time the report is generated it will only have new content."
              checked={values.settings.content.excludeContentInUnsentReport}
              onChange={(e) => {
                setFieldValue('settings.content.excludeContentInUnsentReport', e.target.checked);
              }}
            />
          </Col>
        </Col>
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
