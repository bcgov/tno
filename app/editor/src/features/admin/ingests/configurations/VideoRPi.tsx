import { FormikSelect, FormikText, FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';
import { IIngestModel } from 'hooks/api-editor';
import React from 'react';
import { Col, Row } from 'tno-core';

import { Languages, LoggingLevels, TimeZones } from './constants';
import * as styled from './styled';

export const VideoRPi: React.FC = (props) => {
  const { values } = useFormikContext<IIngestModel>();

  const timeZone = TimeZones.find((t) => t.value === values.configuration.timeZone);
  const language = Languages.find((t) => t.value === values.configuration.language);
  const logLevel = LoggingLevels.find((t) => t.value === values.configuration.logLevel);

  return (
    <styled.IngestType>
      <FormikText
        label="Device Hostname"
        name="configuration.hostname"
        value={values.configuration.hostname}
        tooltip="Only devices specifically with this hostname will run this ingest"
      />
      <FormikSelect
        label="Timezone"
        name="configuration.timeZone"
        options={TimeZones}
        value={timeZone}
        required
      />
      <FormikSelect
        label="Language"
        name="configuration.language"
        options={Languages}
        value={language}
      />
      <p>
        FFmpeg is used to capture audio and video content. For more advanced configuration review{' '}
        <a href="https://www.ffmpeg.org/ffmpeg.html" target="_blank" rel="noreferrer">
          ffmpeg.org
        </a>
        .
      </p>
      <Row>
        <Col flex="1 1 0">
          <FormikSelect
            label="Logging Level"
            name="configuration.logLevel"
            options={LoggingLevels}
            value={logLevel}
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="Thread Queue Size"
            name="configuration.threadQueueSize"
            value={values.configuration.threadQueueSize}
            type="number"
            placeholder="1024"
          />
        </Col>
      </Row>
      <Row>
        <FormikText
          label="Video Format"
          name="configuration.videoInputFormat"
          placeholder="v4l2"
          value={values.configuration.videoInputFormat}
        />
        <FormikText
          label="Framerate"
          name="configuration.inputFramerate"
          value={values.configuration.inputFramerate}
          type="number"
          placeholder="30"
          size={5}
        />
        <Col flex="1 1 0">
          <FormikText
            label="Video Input Device"
            name="configuration.videoInput"
            tooltip="The input device for video"
            placeholder="/dev/video0"
            value={values.configuration.videoInput}
          />
        </Col>
      </Row>
      <Row>
        <FormikText
          label="Audio Format"
          name="configuration.audioInputFormat"
          placeholder="alsa"
          value={values.configuration.audioInputFormat}
        />
        <FormikText
          label="Channels"
          name="configuration.audioChannels"
          value={values.configuration.audioChannels}
          type="number"
          placeholder="2"
          size={5}
        />
        <Col flex="1 1 0">
          <FormikText
            label="Audio Input Device"
            name="configuration.audioInput"
            tooltip="The input device for audio"
            placeholder="hw:CARD=HDMI,DEV=0"
            value={values.configuration.audioInput}
          />
        </Col>
      </Row>
      <Row>
        <Col flex="1 1 0">
          <FormikText
            label="Volume"
            name="configuration.volume"
            tooltip="Volume in percent or dB (1 = 100%)"
            placeholder="1"
            value={values.configuration.volume}
          />
        </Col>
        <Col flex="1 1 0">
          <FormikText
            label="Output Format"
            name="configuration.outputFormat"
            tooltip="Format of the output file"
            value={values.configuration.outputFormat}
          />
        </Col>
      </Row>
      <FormikTextArea
        label="Other Arguments"
        name="configuration.otherArgs"
        tooltip="Any other arguments to pass to the command"
        placeholder="-c:v libx264 -b:v 1600k -preset ultrafast -x264opts keyint=50 -g 25 -pix_fmt yuv420p -c:a aac -b:a 128k"
        value={values.configuration.otherArgs}
      />
      <p>Use "{'{schedule.Name}.mp4'}" to name the file with the schedule name.</p>
      <FormikText
        label="Output File Name"
        name="configuration.fileName"
        tooltip="Output filename"
        placeholder="{schedule.Name}.mp4"
        value={values.configuration.fileName}
      />
    </styled.IngestType>
  );
};
