import {
  FaClipboard,
  FaComment,
  FaFire,
  FaHeart,
  FaHome,
  FaSlidersH,
  FaUsers,
  FaUserTie,
} from 'react-icons/fa';
import { MenuItemNames } from './MenuItemNames';

export interface SidebarMenuItem {
  label: MenuItemNames;
  path: string;
  icon: React.ReactNode;
}

export const SidebarMenuItems: SidebarMenuItem[] = [
  {
    label: MenuItemNames.Home,
    path: 'home',
    icon: <FaHome />,
  },
  {
    label: MenuItemNames.TopStories,
    path: 'topstories',
    icon: <FaFire />,
  },
  {
    label: MenuItemNames.MyMinister,
    path: 'myminister',
    icon: <FaUserTie />,
  },
  {
    label: MenuItemNames.TodaysCommentary,
    path: 'todayscommentary',
    icon: <FaComment />,
  },
  {
    label: MenuItemNames.PressGallery,
    path: 'pressgallery',
    icon: <FaUsers />,
  },
  {
    label: MenuItemNames.FilterMedia,
    path: 'filtermedia',
    icon: <FaSlidersH />,
  },
  {
    label: MenuItemNames.MyCollections,
    path: 'mycollections',
    icon: <FaHeart />,
  },
  {
    label: MenuItemNames.MyReports,
    path: 'myreports',
    icon: <FaClipboard />,
  },
];
