import React from 'react';
import { FaEnvelope } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { Tooltip } from 'react-tooltip';
import { useColleagues, useLookup } from 'store/hooks';
import {
  Button,
  IContentModel,
  IUserColleagueModel,
  Modal,
  Row,
  Settings,
  Text,
  useModal,
} from 'tno-core';

import * as styled from './styled';

export interface IShareSubMenuProps {
  /**
   * The content that has been selected to add to a folder
   */
  content: IContentModel[];
}

/** Component that renders the button that gives users access to a sub menu that will allow them to add selected content to
 * an existing folder. Or create a new one.
 */
export const ShareMenu: React.FC<IShareSubMenuProps> = ({ content }) => {
  const [{ settings }] = useLookup();
  const { toggle, isShowing } = useModal();
  const [{ getColleagues, share, shareEmail }] = useColleagues();
  const [options, setOptions] = React.useState<IUserColleagueModel[]>([]);
  const [user, setUser] = React.useState<IUserColleagueModel | null>(null);
  const [emailAddress, setEmailAddress] = React.useState<string>('');

  const handleSendColleague = async () => {
    try {
      const notificationId = settings.find((s) => s.name === Settings.DefaultAlert)?.value;
      if (notificationId) {
        content.forEach(async (c) => {
          if (user) {
            await share(c.id, user.colleagueId, parseInt(notificationId));
          }
        });
        toast.success('Notification has been successfully requested');
      } else {
        toast.error(`${Settings.DefaultAlert} setting needs to be configured.`);
      }
    } catch {}
  };

  const handleSendEmail = async () => {
    try {
      const notificationId = settings.find((s) => s.name === Settings.DefaultAlert)?.value;
      if (notificationId) {
        content.forEach(async (c) => {
          if (emailAddress !== '') {
            const resp = await shareEmail(c.id, emailAddress, parseInt(notificationId));
            if (resp) {
              toast.success('Notification has been successfully requested');
            }
          }
        });
      } else {
        toast.error(`${Settings.DefaultAlert} setting needs to be configured.`);
      }
    } catch {}
  };

  React.useEffect(() => {
    if (!options.length) {
      getColleagues().then((data) => {
        setOptions(data);
      });
    }
  }, [options.length, getColleagues]);

  const message =
    content.length > 0
      ? `Share ${content.length} selected content${content.length > 1 ? 's' : ''} with ${
          emailAddress !== '' ? emailAddress : user?.colleague?.email
        } ?`
      : `Please select stories to share with your colleague.`;

  return (
    <styled.ShareMenu className="share-sub-menu">
      <Row justifyContent="end" className="action">
        <div className="action" data-tooltip-id="share">
          <FaEnvelope /> SHARE
        </div>
      </Row>
      <Tooltip
        clickable
        variant="light"
        className="share-menu"
        place="bottom"
        openOnClick
        style={{ opacity: '1', boxShadow: '0 0 8px #464545', zIndex: '999' }}
        id="share"
      >
        <FaEnvelope /> SHARE WITH A COLLEAGUE:
        <ul>
          {options.map((o) => {
            return (
              <li
                key={o.colleague?.email}
                onClick={() => {
                  setEmailAddress('');
                  setUser(o);
                  toggle();
                }}
              >
                {o.colleague?.displayName !== '' ? o.colleague?.displayName : o.colleague?.email}
              </li>
            );
          })}
        </ul>
        <Row className="add-row">
          <Text
            placeholder="Share with email..."
            name="email"
            onChange={(e) => setEmailAddress(e.target.value)}
          />
          <Button
            className="share-email"
            onClick={() => {
              setUser(null);
              toggle();
            }}
          >
            Share
          </Button>
        </Row>
      </Tooltip>
      <Modal
        headerText="Share Content"
        body={message}
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Share"
        enableConfirm={
          content.length > 0 && (user?.colleague?.email !== undefined || emailAddress !== '')
        }
        onConfirm={() => {
          emailAddress !== '' ? handleSendEmail() : handleSendColleague();
          toggle();
        }}
      />
    </styled.ShareMenu>
  );
};
