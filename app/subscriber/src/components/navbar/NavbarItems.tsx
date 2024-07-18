import {
  FaClipboard,
  FaCog,
  FaCogs,
  FaComment,
  FaEnvelope,
  FaFire,
  FaFolder,
  FaHome,
  FaMoon,
  FaNewspaper,
  FaQuestion,
  FaSearch,
  FaSlidersH,
  FaSun,
  FaUsers,
  FaUserTie,
} from 'react-icons/fa';
import { IContentState } from 'store/slices';

export interface INavbarOptionItem {
  label: string;
  path: string;
  icon: JSX.Element;
  groupName?: string;
  secondaryIcon?: JSX.Element;
  secondaryIconRoute?: string;
  reduxFilterStore?: keyof IContentState;
}

export interface INavbarOptions {
  [key: string]: INavbarOptionItem;
}

export const NavbarOptions: INavbarOptions = {
  home: {
    label: 'Featured Stories',
    path: 'landing/home',
    icon: <FaHome />,
    reduxFilterStore: 'home',
  },
  topStories: {
    label: 'Top Stories',
    path: 'landing/topstories',
    icon: <FaFire />,
    reduxFilterStore: 'topStories',
  },
  todaysCommentary: {
    label: "Today's Commentary",
    path: 'landing/todayscommentary',
    icon: <FaComment />,
    reduxFilterStore: 'todaysCommentary',
  },
  todaysFrontPages: {
    label: "Today's Front Pages",
    path: 'landing/todaysfrontpages',
    icon: <FaNewspaper />,
  },
  pressGallery: {
    label: 'Press Gallery',
    path: 'landing/pressgallery',
    icon: <FaUsers />,
  },
  eventOfTheDay: {
    label: 'AM Analysis',
    path: 'landing/eventoftheday',
    icon: <FaSun />,
  },
  eveningOverview: {
    label: 'Evening Overview',
    path: 'landing/eveningoverview',
    icon: <FaMoon />,
  },
  filterMedia: {
    label: 'Filter by media type',
    path: 'filter-media',
    icon: <FaSlidersH />,
  },
  myProducts: {
    label: 'MMI Products',
    path: 'products',
    icon: <FaEnvelope />,
  },
  myMinister: {
    label: 'My Minister',
    path: 'landing/myminister',
    icon: <FaUserTie />,
    secondaryIcon: <FaCog />,
    groupName: 'MY CONTENT',
    secondaryIconRoute: '/landing/settings',
  },
  mySearches: {
    groupName: 'MY CONTENT',
    label: 'My Saved Searches',
    path: 'landing/mysearches',
    icon: <FaSearch />,
  },
  folders: {
    groupName: 'MY CONTENT',
    label: 'My Folders',
    path: 'folders',
    icon: <FaFolder />,
  },
  myReports: {
    groupName: 'MY CONTENT',
    label: 'My Reports',
    path: 'reports',
    icon: <FaClipboard />,
  },
  help: {
    groupName: 'USER RESOURCES',
    label: 'Help',
    path: 'help',
    icon: <FaQuestion />,
  },
  settings: {
    groupName: 'USER RESOURCES',
    label: 'Settings',
    path: 'settings',
    icon: <FaCogs />,
  },
};

export const navbarOptions = Object.keys(NavbarOptions).map((key) => NavbarOptions[key]);
