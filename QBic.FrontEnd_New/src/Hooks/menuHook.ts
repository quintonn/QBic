import { useEffect, useState } from "react";
import { SideNavigationProps } from "@cloudscape-design/components";
import { TestMenuData } from "../TestData/Menus";

export interface MenuItem {
  Name: string;
  ParentMenu: null;
  SubMenus: MenuItem[] | null;
  Event: number | null;
  Position: number;
  Id: string;
  CanDelete: boolean;
}

export interface AppMenuItem {
  href: string;
  path: string;
  name: string;
  parentMenu: any;
  subMenus: AppMenuItem[] | null;
  event: number | null;
  position: number;
  id: string;
  canDelete: boolean;
}

const MapMenuItemsToSideNavItems = (
  items: MenuItem[],
  root: boolean = true
): SideNavigationProps.Item[] => {
  const results: SideNavigationProps.Item[] = [];

  if (root) {
    results.push({ text: "Home", href: "#", type: "link" });
  }

  for (let i = 0; i < items.length; i++) {
    const item = items[i];
    if (item.SubMenus && item.SubMenus.length > 0) {
      const sectionItem = {
        text: item.Name,
        type: "section",
        items: MapMenuItemsToSideNavItems(item.SubMenus, false),
      } as SideNavigationProps.Item;

      results.push(sectionItem);
    } else {
      results.push({ text: item.Name, href: "#" + item.Id, type: "link" });
    }
  }

  return results;
};

const MapMenuItemToAppMenuItem = (
  items: MenuItem[],
  path: string = ""
): AppMenuItem[] => {
  if (!items) {
    return [];
  }
  return items.map((item) => ({
    path: path + "/" + item.Id,
    href: item.Id,
    id: item.Id,
    name: item.Name,
    parentMenu: null,
    subMenus: [
      {
        id: "#back",
        event: null,
        position: 0,
        canDelete: false,
        href: "#back/" + item.Id,
        name: "<<",
        parentMenu: null,
        path: "",
        subMenus: [],
      },
      ...MapMenuItemToAppMenuItem(item.SubMenus, path + "/" + item.Id),
    ],
    event: item.Event,
    position: item.Position,
    canDelete: item.CanDelete,
  }));
};

export const useMenus = () => {
  const [appMenuItems, setAppMenuItems] = useState<AppMenuItem[]>([]);
  const [sideNavMenuItems, setSideNavMenuItems] = useState<
    SideNavigationProps.Item[]
  >([]);

  useEffect(() => {
    //TODO: fetch this async etc.

    setTimeout(() => {
      const menuItems = MapMenuItemToAppMenuItem(
        TestMenuData as MenuItem[],
        ""
      );

      const sideNavItems = MapMenuItemsToSideNavItems(
        TestMenuData as MenuItem[]
      );

      setAppMenuItems(menuItems);
      setSideNavMenuItems(sideNavItems);
    });
  }, []);

  return { appMenuItems, sideNavMenuItems };
};
