import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { filterEnabled } from 'store/hooks/lookup/utils';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  OptionItem,
  Row,
} from 'tno-core';

import { FileTypes, Languages, TimeZones } from './constants';
import * as styled from './styled';

export const Newspaper: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();
  useTooltips();

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);
  const language = Languages.find((t) => t.value === values.configuration.language);
  const fileType = FileTypes.find((t) => t.value === values.configuration.fileFormat);
  const [lookups] = useLookup();
  const sources = lookups.sources.map((s) => new OptionItem(s.name, s.code, s.isEnabled));
  const source = sources.find((t) => t.value === values.configuration.defaultSource);

  return (
    <styled.IngestType>
      <Row>
        <Col flex="1 1 0">
          <FormikCheckbox label="Self Published" name="configuration.selfPublished" />
          <FormikCheckbox label="Escape Content" name="configuration.escapeContent" />
          <FormikCheckbox label="Add Parent" name="configuration.addParent" />
        </Col>
        <Col flex="1 1 0"></Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikSelect
            label="Timezone"
            name="configuration.timeZone"
            tooltip="Timezone of the source"
            options={TimeZones}
            value={timeZone}
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="Date Offset"
            name="configuration.dateOffset"
            type="number"
            value={0}
            tooltip="Which day to parse (0 = today)"
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikSelect
            label="File Type"
            name="configuration.fileFormat"
            tooltip="The file extension (xml, fms)"
            options={FileTypes}
            value={fileType}
            required
          />
        </Col>
        <Col flex="1 1 0">
          <FormikSelect
            label="Default Language"
            name="configuration.language"
            options={Languages}
            value={language}
            tooltip="The language of the content unless specified by the path below"
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikText
            label="Path to Files"
            name="configuration.path"
            value={values.configuration.path}
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="File Name Pattern"
            name="configuration.filePattern"
            tooltip="The filename expression"
          />
        </Col>
      </Row>
      <p>
        The following provide a path within the newspaper file to extract data for each content
        entry.
      </p>
      <Row>
        <Col flex="1 1 0">
          <FormikText
            label="Entry"
            name="configuration.item"
            tooltip="The path to the content item (i.e. story)"
            required
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="UID"
            name="configuration.id"
            tooltip="The path to the content id"
            required
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikText
            label="Published Date"
            name="configuration.date"
            tooltip="The path to the published date"
            required
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="Date Format"
            name="configuration.dateFmt"
            tooltip="The format of the published date"
            placeholder="MM-dd-yyyy"
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikText
            label="Headline"
            name="configuration.headline"
            tooltip="The path to the headline"
            required
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="Summary"
            name="configuration.summary"
            tooltip="The path to the summary"
            required
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikText
            label="Paper Name"
            name="configuration.papername"
            tooltip="The path to the paper name"
            required
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="Content"
            name="configuration.story"
            tooltip="The path to the content body or full story"
            required
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikText
            label="Section"
            name="configuration.section"
            tooltip="The path to the section"
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText label="Page" name="configuration.page" tooltip="The path to the page" />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikText label="Author" name="configuration.author" tooltip="The path to the author" />
        </Col>
        <Col flex="1 1 0">
          <FormikText label="Tags" name="configuration.tags" tooltip="The path to the tags" />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikText
            label="Language"
            name="configuration.lang"
            tooltip="The path to the language"
          />
        </Col>
        <Col flex="1 1 0"></Col>
      </Row>
      <FormikTextArea
        label="Sources"
        name="configuration.sources"
        tooltip="Delimited list of paper names and their related source code ({papername}={source}&{papername}={source})"
        placeholder="paper name=source&paper name=source"
      />
      <FormikSelect
        label="Default Source"
        name="configuration.defaultSource"
        tooltip="Source to use for publications missing from Sources list. If not set, articles will be discarded"
        options={filterEnabled(sources)}
        value={source}
      />
    </styled.IngestType>
  );
};
